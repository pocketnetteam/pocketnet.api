﻿using api.Repositories;
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

        public async Task<IEnumerable<Getlastcomments>> GetlastcommentsAsync(string address, string lang, int resultCount = 10)
        {
            if ( string.IsNullOrEmpty (lang)) { lang = "en"; }

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
            _context.Cmd.Parameters.AddWithValue("$address", address).SqliteType= Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$lang", lang).SqliteType = Microsoft.Data.Sqlite.SqliteType.Text;
            _context.Cmd.Parameters.AddWithValue("$resultCount", resultCount).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;
            _context.Cmd.Parameters.AddWithValue("$unixTime", unixTime).SqliteType = Microsoft.Data.Sqlite.SqliteType.Integer;



            List<Getlastcomments> res = new List<Getlastcomments>();

            using (var reader = await _context.Cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    res.Add(new Getlastcomments()
                    {
                        id = reader.SafeGetString("otxid"),
                        postid = reader.SafeGetString("postid"),
                        address = reader.SafeGetString("address"),
                        time = reader.SafeGetInt32("ocmntTime"),
                        timeUpd = reader.SafeGetInt32("time"),
                        block = reader.SafeGetInt32("block"),
                        msg = reader.SafeGetString ("msg") ,// .GetString(6),
                        parentid = reader.SafeGetString("parentid"),
                        answerid = reader.SafeGetString("answerid"),
                        scoreUp = reader.SafeGetInt32("scoreUp"),
                        scoreDown = reader.SafeGetInt32("scoreDown"),
                        reputation = reader.SafeGetInt32("reputation"),
                        edit = (reader.SafeGetString("txid") != reader.SafeGetString("otxid")) ? 1 : 0,
                        deleted = (reader.SafeGetString("msg") == "") ? 1 : 0,
                        myScore = reader.SafeGetInt32("myScore", 0) // reader.GetInt32(13)
                    });

                }
            }

            return res;
        }


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
