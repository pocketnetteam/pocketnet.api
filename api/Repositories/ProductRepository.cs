using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTOs;
using api.Extensions;
using api.Repositories.Interfaces;
using api.Services;
using DynaCache.Attributes;
using Microsoft.Data.Sqlite;

namespace api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogContext _context;

        public ProductRepository(CatalogContext catalogContext)
        {
            _context = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<UserProfile>> GetUserProfileAsync(string addresses, bool shortForm = true, int option = 0)
        {
            var addressLst = addresses.FromJArray();

            // In full form add other fields
            // Subscribes
            // Subscribers TODO limited by 500 - we r not going to fetch all 10mln
            // Blockings

            // TODO typo in subscribes - "adddress"
            // TODO for private in subscribes - SQLite does not have a separate Boolean storage class. Instead, Boolean values are stored as integers 0 (false) and 1 (true).

            var longForm = shortForm  ? "" : @"
     ,regdate
     ,lang
     ,url
     ,time
     ,pubkey

     ,(SELECT json_group_array(json_object( 'adddress', address_to, 'private', private )) AS json_result
     FROM (select address_to,private from SubscribesView sv where sv.address =uw.address))subscribes

     ,(SELECT json_group_array(address) AS json_result
     FROM (select address from SubscribesView sv where sv.address_to =uw.address limit 500))subscribers

     ,(SELECT json_group_array(address_to) AS json_result
     FROM (select address_to from BlockingView bv where bv.address =bv.address))blocking";


            // Minimal fields for short form
            var commandText = $@"select address
     ,id
     ,name
     ,avatar
     ,donations
     ,referrer
     ,reputation/10 as reputation
     ,about
     ,(select count(*)  from Posts p where p.address=uw.address ) postcnt
     ,(select count(*)  from UsersView uw2 where uw2.referrer=uw.address ) referrals_count

     {longForm}

from UsersView uw where address in ('{string.Join("','", addressLst)}')";


            // Recommendations subscribtions
            // TODO now commented out

            var res = new List<UserProfile>();

            var command = new SqliteCommand(commandText, _context.Connection);
            
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                UserProfile up = new UserProfile()
                {
                    address = reader.SafeGetString("address"),
                    name = reader.SafeGetString("name"),
                    id = reader.SafeGetInt32("id"),
                    i = reader.SafeGetString("avatar"),
                    b = reader.SafeGetString("donations"),
                    r = reader.SafeGetString("referrer"),
                    reputation = reader.SafeGetInt32("reputation"),
                    postcnt = reader.SafeGetInt32("postcnt"),
                    rc = reader.SafeGetInt32("referrals_count"),
                };


                if (option == 1)
                {
                    up.a = reader.SafeGetString("about");
                }

                if (!shortForm)
                {
                    up.regdate = reader.SafeGetInt32("regdate");
                    up.l = reader.SafeGetString("lang");
                    up.s = reader.SafeGetString("url");
                    up.update = reader.SafeGetInt32("time");
                    up.k = reader.SafeGetString("pubkey");

                    up.subscribes = Newtonsoft.Json.JsonConvert.DeserializeObject<Subscription[]>(reader.SafeGetString("subscribes"));
                    up.subscribers = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.SafeGetString("subscribers"));
                    up.blocking = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.SafeGetString("blocking"));
                }

                res.Add(up);
            }


            return res;
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<Comment>> GetLastCommentsAsync(string address, string lang, int resultCount = 100)
        {
            var foo = DateTime.UtcNow;
            var unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            var commandText = @"select 
c.txid,
c.otxid,
c.postId,
c.address,
c.time,
c.block,
c.msg,
c.parentId,
c.answerid,
c.scoreUp,
c.scoreDown,
c.reputation   
       ,(select ci.time from Comment ci where ci.txid=c.otxid order by ci.time desc limit 1)ocmntTime
       ,(select cs.value from CommentScores cs where cs.commentid=c.otxid and cs.address=$address order by cs.time desc limit 1)myScore
       from Comment c, Posts p
where
      p.txid=c.postId and
      p.lang=$lang and
      c.time<=$unixTime and
      c.last=1
order by c.time asc
limit $resultCount";
            
            var command = new SqliteCommand(commandText, _context.Connection);

            command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$lang", lang).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = SqliteType.Integer;
            command.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = SqliteType.Integer;

            var res = new List<Comment>();

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                res.Add(new Comment()
                {
                    id = reader.SafeGetString("otxid"),
                    postid = reader.SafeGetString("postId"),
                    address = reader.SafeGetString("address"),
                    time = reader.SafeGetInt32("ocmntTime"),
                    timeUpd = reader.SafeGetInt32("time"),
                    block = reader.SafeGetInt32("block"),
                    msg = reader.SafeGetString("msg"),
                    parentid = reader.SafeGetString("parentId"),
                    answerid = reader.SafeGetString("answerid"),
                    scoreUp = reader.SafeGetInt32("scoreUp"),
                    scoreDown = reader.SafeGetInt32("scoreDown"),
                    reputation = reader.SafeGetInt32("reputation"),
                    edit = (reader.SafeGetString("txid") != reader.SafeGetString("otxid")),
                    deleted = (reader.SafeGetString("msg") == ""),
                    myScore = reader.SafeGetInt32("myScore", 0)
                });

            }


            return res;
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<Comment>> GetCommentsAsync(string postId, string parentId, string address, string commentIds, int resultCount = 100)
        {
            var foo = DateTime.UtcNow;
            var unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            var commentIdsLst = commentIds.FromJArray();

            var where = commentIdsLst.Count == 0
                ? @"c.postId = $postId and c.parentId = $parentId and"
                : $@"c.otxid in ('{string.Join("','", commentIdsLst)}') and";

            var commandText = $@"select c.txid,
c.otxid,
c.postId,
c.address,
c.time,
c.block,
c.msg,
c.parentId,
c.answerid,
c.scoreUp,
c.scoreDown,
c.reputation
       ,(select ci.time from Comment ci where ci.txid=c.otxid order by ci.time desc limit 1)ocmntTime
       ,(select cs.value from CommentScores cs where cs.commentid=c.otxid and cs.address=$address order by cs.time desc limit 1)myScore
       ,(select count(*) from Comment ci where ci.parentId=c.otxid and ci.last=1)children
from Comment c
where
      {where}
      c.time<=$unixTime and
      c.last=1
order by c.time asc
limit $resultCount";

            var command = new SqliteCommand(commandText, _context.Connection);

            if (commentIdsLst.Count == 0)
            {
                command.Parameters.AddWithValue("$postId", postId).SqliteType = SqliteType.Text;
                command.Parameters.AddWithValue("$parentId", parentId).SqliteType = SqliteType.Text;
            }

            command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = SqliteType.Integer;
            command.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = SqliteType.Integer;

            var res = new List<Comment>();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new Comment()
                {
                    id = reader.SafeGetString("otxid"),
                    postid = reader.SafeGetString("postId"),
                    address = reader.SafeGetString("address"),
                    time = reader.SafeGetInt32("ocmntTime"),
                    timeUpd = reader.SafeGetInt32("time"),
                    block = reader.SafeGetInt32("block"),
                    msg = reader.SafeGetString("msg"),
                    parentid = reader.SafeGetString("parentId"),
                    answerid = reader.SafeGetString("answerid"),
                    scoreUp = reader.SafeGetInt32("scoreUp"),
                    scoreDown = reader.SafeGetInt32("scoreDown"),
                    reputation = reader.SafeGetInt32("reputation"),
                    edit = (reader.SafeGetString("txid") != reader.SafeGetString("otxid")),
                    deleted = (reader.SafeGetString("msg") == ""),
                    myScore = reader.SafeGetInt32("myScore", 0),
                    children = reader.SafeGetString("children", "0")
                });
            }

            return res;
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<Score>> GetPageScoresAsync(string txIds, string address, string commentIds, int resultCount = 100)
        {
            var txIdsLst = txIds.FromJArray();
            var commentIdsLst = commentIds.FromJArray();

            var foo = DateTime.UtcNow;
            var unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            var commandText = $@"select c.txid,
c.otxid,
c.scoreUp,
c.scoreDown,
c.reputation
       ,(select cs.value from CommentScores cs where cs.commentid = c.otxid and cs.address = $address order by cs.time desc limit 1)myScore
    from Comment c --INDEXED BY Comment_otxid_last_time_index
where
      c.otxid in ('{string.Join("','", commentIdsLst)}') and
      c.time <= $unixTime and
      c.last = 1
order by c.time asc
limit $resultCount";

            var command = new SqliteCommand(commandText, _context.Connection);

            command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = SqliteType.Integer;
            command.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = SqliteType.Integer;

            var res = new List<Score>();

            // TODO
            //1. Add postlikers from private subscribes
            //2. Add postlikers from not private subscribes
            //3. Add postlikers from not subscribes

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                res.Add(new Score()
                {
                    cmntid = reader.SafeGetString("otxid"),
                    scoreUp = reader.SafeGetInt32("scoreUp"),
                    scoreDown = reader.SafeGetInt32("scoreDown"),
                    reputation = reader.SafeGetInt32("reputation"),
                    myScore = reader.SafeGetInt32("myScore", 0)
                });
            }

            return res;
        }
    }
}
