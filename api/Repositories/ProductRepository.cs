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
    from Comment c INDEXED BY Comment_otxid_last_time_index
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
