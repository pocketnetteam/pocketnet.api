using api.DTOs;
using api.Repositories;
using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Catalog.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ICatalogContext _context;

        public ProductRepository(ICatalogContext catalogContext)
        {
            _context = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            List<Product> res = new List<Product>();

            using (var reader = await _context.CommandExecutor("select Id, Name , Category,  Summary , Description , Price from Products"))
            {
                while (reader.Read())
                {
                    res.Add(new Product()
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Category = reader.GetString(2),
                        Summary = reader.GetString(3),
                        Description = reader.GetString(4),
                        Price = reader.GetInt32(5)
                    });

                }
            }

            return res;
        }

        public async Task<IEnumerable<Comment>> GetlastcommentsAsync(string address, string lang, int resultCount = 100)
        {
            if (string.IsNullOrEmpty(lang)) { lang = "en"; }

            DateTime foo = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            _context.Cmd.CommandText = @"select 
c.txid,
c.otxid,
c.postid,
c.address,
c.time,
c.block,
c.msg,
c.parentid,
c.answerid,
c.scoreUp,
c.scoreDown,
c.reputation   
       ,(select ci.time from Comment ci where ci.txid=c.otxid order by ci.time desc limit 1)ocmntTime
       ,(select cs.value from CommentScores cs where cs.commentid=c.otxid and cs.address=$address order by cs.time desc limit 1)myScore
       from Comment c, Posts p
where
      p.txid=c.postid and
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

            List<Comment> res = new List<Comment>();

            using (var reader = await _context.Cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    res.Add(new Comment()
                    {
                        id = reader.SafeGetString("otxid"),
                        postid = reader.SafeGetString("postid"),
                        address = reader.SafeGetString("address"),
                        time = reader.SafeGetInt32("ocmntTime"),
                        timeUpd = reader.SafeGetInt32("time"),
                        block = reader.SafeGetInt32("block"),
                        msg = reader.SafeGetString("msg"),
                        parentid = reader.SafeGetString("parentid"),
                        answerid = reader.SafeGetString("answerid"),
                        scoreUp = reader.SafeGetInt32("scoreUp"),
                        scoreDown = reader.SafeGetInt32("scoreDown"),
                        reputation = reader.SafeGetInt32("reputation"),
                        edit = (reader.SafeGetString("txid") != reader.SafeGetString("otxid")),
                        deleted = (reader.SafeGetString("msg") == ""),
                        myScore = reader.SafeGetInt32("myScore", 0)
                    });

                }
            }

            return res;
        }

        public async Task<IEnumerable<Comment>> GetcommentsAsync(string postid, string parentid, string address, string comment_ids, int resultCount = 100)
        {
            DateTime foo = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

            if (string.IsNullOrEmpty(postid)) { postid = ""; }
            if (string.IsNullOrEmpty(parentid)) { parentid = ""; }
            if (string.IsNullOrEmpty(address)) { address = ""; }
            if (string.IsNullOrEmpty(comment_ids)) { comment_ids = ""; }

            List<string> comment_idsLst = comment_ids.FromJArray();

            _context.Cmd.Parameters.Clear();

            _context.Cmd.CommandText = @"select c.txid,
c.otxid,
c.postid,
c.address,
c.time,
c.block,
c.msg,
c.parentid,
c.answerid,
c.scoreUp,
c.scoreDown,
c.reputation
       ,(select ci.time from Comment ci where ci.txid=c.otxid order by ci.time desc limit 1)ocmntTime
       ,(select cs.value from CommentScores cs where cs.commentid=c.otxid and cs.address=$address order by cs.time desc limit 1)myScore
       ,(select count(*) from Comment ci where ci.parentid=c.otxid and ci.last=1)children
from Comment c
where
      $WHERE$
      c.time<=$unixTime and
      c.last=1
order by c.time asc
limit $resultCount";

            if (comment_idsLst.Count == 0)
            {
                _context.Cmd.CommandText = _context.Cmd.CommandText.Replace("$WHERE$", @"c.postid = $postid and c.parentid = $parentid and");

                _context.Cmd.Parameters.AddWithValue("$postid", postid).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
                _context.Cmd.Parameters.AddWithValue("$parentid", parentid).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            }
            else
            {
                _context.Cmd.CommandText = _context.Cmd.CommandText.Replace("$WHERE$", @"c.otxid in ({0}) and");
                _context.Cmd.CommandText = String.Format(_context.Cmd.CommandText, $"'{string.Join("','", comment_idsLst)}'");
            }

            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;

            List<Comment> res = new List<Comment>();
            
            using (var reader = await _context.Cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    res.Add(new Comment()
                    {
                        id = reader.SafeGetString("otxid"),
                        postid = reader.SafeGetString("postid"),
                        address = reader.SafeGetString("address"),
                        time = reader.SafeGetInt32("ocmntTime"),
                        timeUpd = reader.SafeGetInt32("time"),
                        block = reader.SafeGetInt32("block"),
                        msg = reader.SafeGetString("msg"),
                        parentid = reader.SafeGetString("parentid"),
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
            }
            
            return res;
        }


        public async Task<IEnumerable<Score>> GetpagescoresAsync(string tx_ids, string address, string comment_ids, int resultCount = 100)
        {
            //File.AppendAllText(@"D:\Work\iNET\pocketnet.api\api\bin\Debug\time", $"\r\n{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}: 1");

            if (string.IsNullOrEmpty(tx_ids)) { tx_ids = ""; }
            if (string.IsNullOrEmpty(address)) { address = ""; }
            if (string.IsNullOrEmpty(comment_ids)) { comment_ids = ""; }

            List<string> tx_idsLst = tx_ids.FromJArray();
            List<string> comment_idsLst = comment_ids.FromJArray();

            //File.AppendAllText(@"D:\Work\iNET\pocketnet.api\api\bin\Debug\time", $"\r\n{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}: 2");// 4 ms

            DateTime foo = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();


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

            //File.AppendAllText(@"D:\Work\iNET\pocketnet.api\api\bin\Debug\time", $"\r\n{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}: 3");// 5 ms

            _context.Cmd.Parameters.Clear();
            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.CommandText = String.Format(_context.Cmd.CommandText, $"'{string.Join("','", comment_idsLst)}'");

            //File.AppendAllText(@"D:\Work\iNET\pocketnet.api\api\bin\Debug\time", $"\r\n{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}: 4");// 4 ms

            List<Score> res = new List<Score>();

            // TODO
            //1. Add postlikers from private subscribes
            //2. Add postlikers from not private subscribes
            //3. Add postlikers from not subscribes



            using (var reader = await _context.Cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
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
            }
            //File.AppendAllText(@"D:\Work\iNET\pocketnet.api\api\bin\Debug\time", $"\r\n{DateTime.Now.Minute}:{DateTime.Now.Second}:{DateTime.Now.Millisecond}: 5");//166 ms

            return res;

            /*
        cmntscore.pushKV("cmntid", cmntItm["otxid"].As<string>());
        cmntscore.pushKV("scoreUp", cmntItm["scoreUp"].As<string>());
        cmntscore.pushKV("scoreDown", cmntItm["scoreDown"].As<string>());
        cmntscore.pushKV("reputation", cmntItm["reputation"].As<string>());
        if (cit.GetJoined().size() > 0) cmntscore.pushKV("myscore", cit.GetJoined()[0][0].GetItem()["value"].As<string>());
            */
        }
     
    }
}
