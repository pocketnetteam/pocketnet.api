using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
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

            return  res;
        }

        public async Task<IEnumerable<Getlastcomments>> GetlastcommentsAsync(string address, string lang)
        {

            if (lang == "") { lang = "en"; }

            _context.Cmd.CommandText = @"select 
c.txid,
c.otxid,
c.last,
c.postid,
c.address,
c.time,
c.block,
c.msg,
c.parentid/*,
c.answerid,
c.scoreUp,
c.scoreDown,
c.reputation */  
       ,(select ci.time from Comment ci where ci.txid=c.otxid order by ci.time desc limit 1)ocmntTime
       ,(select cs.value from CommentScores cs where cs.commentid=c.otxid and cs.address=$address order by cs.time desc limit 1)myScore
       from Comment c, Posts p
where
      p.txid=c.postid and
      p.lang=$lang and
      c.time<=1610615221 and
      c.last=1
order by c.time asc
limit 10";

            _context.Cmd.Parameters.Clear();
            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType= Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$lang", lang).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;


            List<Getlastcomments> res = new List<Getlastcomments>();

            using (var reader = await _context.Cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    res.Add(new Getlastcomments()
                    {
                        id = reader.GetString(0),
                        postid = reader.GetString(1),
                        address = reader.GetString(2)
                        //Summary = reader.GetString(2),
                        //Description = reader.GetString(3),
                        //Price = reader.GetInt32(4)
                        /*
                                oCmnt.pushKV("id", cmntItm["otxid"].As<string>());
                    oCmnt.pushKV("postid", cmntItm["postid"].As<string>());
                    oCmnt.pushKV("address", cmntItm["address"].As<string>());
                    oCmnt.pushKV("time", ocmntItm["time"].As<string>());
                    oCmnt.pushKV("timeUpd", cmntItm["time"].As<string>());
                    oCmnt.pushKV("block", cmntItm["block"].As<string>());
                    oCmnt.pushKV("msg", cmntItm["msg"].As<string>());
                    oCmnt.pushKV("parentid", cmntItm["parentid"].As<string>());
                    oCmnt.pushKV("answerid", cmntItm["answerid"].As<string>());
                    oCmnt.pushKV("scoreUp", cmntItm["scoreUp"].As<string>());
                    oCmnt.pushKV("scoreDown", cmntItm["scoreDown"].As<string>());
                    oCmnt.pushKV("reputation", cmntItm["reputation"].As<string>());
                    oCmnt.pushKV("edit", cmntItm["otxid"].As<string>() != cmntItm["txid"].As<string>());
                    oCmnt.pushKV("deleted", cmntItm["msg"].As<string>() == "");
                    oCmnt.pushKV("myScore", myScore);
                        */

                });

                }
            }

            return res;
        }

        /*
         select c.*
       ,(select ci.time from Comment ci where ci.txid=c.otxid order by ci.time desc limit 1)ocmntTime
       ,(select cs.value from CommentScores cs where cs.commentid=c.otxid and cs.address='PBaFNz7VNA35GE9rZw6RLqhyECkhENShJi' order by cs.time desc limit 1)myScore
       from Comment c, Posts p
where
      p.txid=c.postid and
      p.lang='en' and
      c.time<=1610615221 and
      c.last=1
order by c.time asc
limit 50
        */


        public async Task<IEnumerable<Product>> GetProductByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Product>> GetProductByCategoryAsync(string categoryName)
        {
            throw new NotImplementedException();
        }

        //public async Task CreateAsync(Product product)
        //{
        //    throw new NotImplementedException();
 
        //}

        //public async Task<bool> UpdateAsync(Product product)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<bool> DeleteAsync(string id)
        //{
        //    throw new NotImplementedException();
        //}        
    }
}
