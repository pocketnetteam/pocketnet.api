using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTOs;
using api.Extensions;
using api.Models;
using api.Repositories.Interfaces;
using api.Services.Interfaces;
using DynaCache.Attributes;

namespace api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICatalogContext _context;

        public ProductRepository(ICatalogContext catalogContext)
        {
            _context = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
        }

        [CacheableMethod(60)]
        public async Task<IEnumerable<Comment>> GetLastCommentsAsync(string address, string lang, int resultCount = 100)
        {
            var foo = DateTime.UtcNow;
            var unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            _context.Cmd.CommandText = @"select 
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

            _context.Cmd.Parameters.Clear();
            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$lang", lang).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;

            var res = new List<Comment>();

            await using var reader = await _context.Cmd.ExecuteReaderAsync();
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
        public async Task<IEnumerable<Comment>> GetCommentsAsync(string postId, string parentId, string address, string commentIds, int resultCount = 100)
        {
            var foo = DateTime.UtcNow;
            var unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            var commentIdsLst = commentIds.FromJArray();

            _context.Cmd.Parameters.Clear();

            _context.Cmd.CommandText = @"select c.txid,
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
      $WHERE$
      c.time<=$unixTime and
      c.last=1
order by c.time asc
limit $resultCount";

            if (commentIdsLst.Count == 0)
            {
                _context.Cmd.CommandText = _context.Cmd.CommandText.Replace("$WHERE$", @"c.postId = $postId and c.parentId = $parentId and");

                _context.Cmd.Parameters.AddWithValue("$postId", postId).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
                _context.Cmd.Parameters.AddWithValue("$parentId", parentId).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            }
            else
            {
                _context.Cmd.CommandText = _context.Cmd.CommandText.Replace("$WHERE$", @"c.otxid in ({0}) and");
                _context.Cmd.CommandText = string.Format(_context.Cmd.CommandText, $"'{string.Join("','", commentIdsLst)}'");
            }

            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;

            var res = new List<Comment>();

            await using var reader = await _context.Cmd.ExecuteReaderAsync();

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
        public async Task<IEnumerable<Score>> GetPageScoresAsync(string txIds, string address, string commentIds, int resultCount = 100)
        {
            var txIdsLst = txIds.FromJArray();
            var commentIdsLst = commentIds.FromJArray();

            var foo = DateTime.UtcNow;
            var unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            _context.Cmd.CommandText = @"select c.txid,
c.otxid,
c.scoreUp,
c.scoreDown,
c.reputation
       ,(select cs.value from CommentScores cs where cs.commentid = c.otxid and cs.address = $address order by cs.time desc limit 1)myScore
    from Comment c
where
      c.otxid in ({0}) and
      c.time <= $unixTime and
      c.last = 1
order by c.time asc
limit $resultCount";


            _context.Cmd.Parameters.Clear();
            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.CommandText = string.Format(_context.Cmd.CommandText, $"'{string.Join("','", commentIdsLst)}'");

            var res = new List<Score>();

            // TODO
            //1. Add postlikers from private subscribes
            //2. Add postlikers from not private subscribes
            //3. Add postlikers from not subscribes

            await using var reader = await _context.Cmd.ExecuteReaderAsync();

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
