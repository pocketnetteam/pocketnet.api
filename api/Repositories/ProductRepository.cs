using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTOs;
using api.Extensions;
using api.Repositories.Interfaces;
using api.Services;
using DynaCache.Attributes;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogContext _context;

        public ProductRepository(CatalogContext catalogContext)
        {
            _context = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
        }

        public virtual async Task<IEnumerable<UserProfile>> GetUserProfileAsync(string addresses, bool shortForm = true, int option = 0)
        {
            var addressLst = addresses.FromJArray();

            return await GetUserProfileAsync(addressLst, shortForm, option);
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<UserProfile>> GetUserProfileAsync(IReadOnlyCollection<string> addresses, bool shortForm = true, int option = 0)
        {

            // In full form add other fields
            // Subscribes
            // Subscribers TODO limited by 500 - we r not going to fetch all 10mln
            // Blockings

            // TODO typo in subscribes - "adddress"

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

from UsersView uw where address in ('{string.Join("','", addresses)}')";


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

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<Tag>> GetTagsAsync(string address, int count, int block, string lang)
        {
            string addressblock  = "";
            if (address!="")   addressblock = "address = $address and";

            var commandText = $@"select p.tags from Posts p where lang=$lang and block>=$block and {addressblock} tags!='[]';";

            // Manual tags Deserialization works 7-10 % faster than DB parsing with json_each
            // var commandText = $@"SELECT lower(t.value)tag, count(*) count FROM Posts p, json_each(p.tags)t where lang=$lang and block>=$block and {addressblock} tags!='[]' group by lower(t.value) order by count(*) desc;";

            var command = new SqliteCommand(commandText, _context.Connection);

            if (address != "") command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$lang", lang).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$block", block).SqliteType = SqliteType.Integer;

            var counter = new Dictionary <string, int>();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var tgs = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.SafeGetString("tags"));
                foreach (var t in tgs.Select (t => t.ToLower ()))
                {
                    counter.TryGetValue(t, out int value);
                    counter[t] = value+1;
                }
            }

            var res = counter.OrderByDescending(t => t.Value).Take(count).Select(t => new Tag()
            {
                tag = t.Key,
                count = t.Value
            }).ToList();

            return res;
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<UserAddress>> GetUserAddressAsync(string name, int count)
        {
            var commandText = $@"select address from UsersView uw where name=$name COLLATE NOCASE limit $count;";

            var command = new SqliteCommand(commandText, _context.Connection);

            command.Parameters.AddWithValue("$name", name).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$count", count).SqliteType = SqliteType.Integer;

            await using var reader = await command.ExecuteReaderAsync();

            var res = new List<UserAddress>();

            while (await reader.ReadAsync())
            {
                UserAddress ua = new UserAddress()
                {
                    name = name,
                    address = reader.SafeGetString("address")
                };

                res.Add(ua);
            }

            return res;
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<Content>> GetContentsAsync(string address, string lang, int count)
        {
            var commandText = $@"select case when caption ='' then message else caption end content, txid,time,reputation,settings,scoreCnt,scoreSum  from Posts p where lang=$lang and address=$address order by time desc limit $count;";

            var command = new SqliteCommand(commandText, _context.Connection);

            command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$lang", lang).SqliteType = SqliteType.Text;
            command.Parameters.AddWithValue("$count", count).SqliteType = SqliteType.Integer;

            await using var reader = await command.ExecuteReaderAsync();

            var res = new List<Content>();

            while (await reader.ReadAsync())
            {
                Content c = new Content()
                {
                    content = reader.SafeGetString("content"),
                    txid = reader.SafeGetString("txid"),
                    time = reader.SafeGetString("time"),
                    reputation = reader.SafeGetString("reputation"),
                    settings = reader.SafeGetString("settings"),
                    scoreSum = reader.SafeGetString("scoreSum"),
                    scoreCnt = reader.SafeGetString("scoreCnt")
                };

                res.Add(c);
            }

            return res;
        }

        [CacheableMethod(60)]
        public virtual async Task<IEnumerable<PostData>> GetRawTransactionWithMessageByIdAsync(string txIds, string address)
        {
            var res = new List<PostData>();

            var txIdsLst = txIds.FromJArray();

            var commandText = $@"select
        txid
       ,case when txidEdit then 1 else 0 end edit
       ,case when txidRepost then txidRepost else '' end repost
       ,address
       ,time
       ,lang
       ,caption
       ,message
       ,url
       ,scoreSum
       ,scoreCnt
       ,tags
       ,images
       ,settings
       ,ifnull((select value from Scores s where posttxid=p.txid and s.address=$address),0)myVal
       ,(select count(*) from Comment c where c.postid =p.txid and c.last=1) comments
       ,(select count(*) from Posts p2 where p2.txidRepost=p.txid)reposted
,(SELECT json_object('id', id, 'postid', postid, 'address', address, 'timeUpd', timeUpd ,  'block', block,
    'msg' ,msg, 'parentid' ,parentid, 'answerid' ,answerid, 'scoreUp' ,scoreUp, 'scoreDown', scoreDown, 'reputation' ,reputation, 'edit' ,edit, 'deleted', deleted, 'time', time, 'myScore', myScore, 'children' ,children) AS json_result
     FROM (
     select c.otxid as id
       ,c.postid
       ,c.address
       ,c.time as timeUpd
       ,c.block
       ,c.msg
       ,c.parentid
       ,c.answerid
       ,c.scoreUp
       ,c.scoreDown
       ,c.reputation
       ,case when c.txid!=c.otxid then 1 else 0 end edit
       ,case when c.msg='' then 1 else 0 end deleted
       ,(select time from Comment c1 where c1.txid= c.otxid limit 1) time
       ,ifnull((select cs.value from CommentScores cs where c.otxid=cs.commentid  and cs.address=$address limit 1),0) myScore
       ,(select count(*) from Comment c2 where c2.parentid=c.otxid and last=1)children
          from Comment c
          where
                c.postid =p.txid and
                parentid='' and
                c.last=1
          order by c.time desc limit 1))lastComment
 from Posts p where txid in ('{string.Join("','", txIdsLst)}');";

            var command = new SqliteCommand(commandText, _context.Connection);

            command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                PostData pd = new PostData()
                {
                    txid = reader.SafeGetString("txid"),
                    txidEdit = reader.SafeGetBool("edit"),
                    txidRepost = reader.SafeGetString("repost"),
                    address = reader.SafeGetString("address"),
                    time = reader.SafeGetInt32("time"),
                    l = reader.SafeGetString("lang"),
                    c = reader.SafeGetString("caption"),
                    m = reader.SafeGetString("message"),
                    u = reader.SafeGetString("url"),
                    scoreSum = reader.SafeGetString("scoreSum"),
                    scoreCnt = reader.SafeGetString("scoreCnt"),
                    myVal = reader.SafeGetString("myVal"),
                    comments=reader.SafeGetInt32("comments"), 
                    reposted = reader.SafeGetInt32("reposted")
                };

                pd.lastComment = Newtonsoft.Json.JsonConvert.DeserializeObject<Comment>(reader.SafeGetString("lastComment"));
                pd.s= Newtonsoft.Json.JsonConvert.DeserializeObject<DTOs.Settings>(reader.SafeGetString("settings"));
                pd.t=Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.SafeGetString("tags"));
                pd.i=Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.SafeGetString("images"));

                pd.userprofile = (await GetUserProfileAsync(new List<string> { pd.address } )).FirstOrDefault();


                res.Add(pd);
            }


            return res;
        }
    }
}
