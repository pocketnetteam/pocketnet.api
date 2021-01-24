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

        public async Task<IEnumerable<Product>> GetProducts()
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

        public async Task<Product> GetProduct(string id)
        {
            Product res = null;

            using (var reader = await _context.CommandExecutor($"select Name , Category,  Summary , Description , Price from Products where id={id}"))
            {
                while (reader.Read())
                {
                    res= new Product()
                    {
                        Name = reader.GetString(0),
                        Category = reader.GetString(1),
                        Summary = reader.GetString(2),
                        Description = reader.GetString(3),
                        Price = reader.GetInt32(4)
                    };

                }
            }

            return res;
        }

        public async Task<IEnumerable<Product>> GetProductByName(string name)
        {
            throw new NotImplementedException();
            /*
            FilterDefinition<Product> filter = Builders<Product>.Filter.ElemMatch(p => p.Name, name);

            return await _context
                          .Products
                          .Find(filter)
                          .ToListAsync();
            */
        }

        public async Task<IEnumerable<Product>> GetProductByCategory(string categoryName)
        {
            throw new NotImplementedException();
            /*
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Category, categoryName);

            return await _context
                          .Products
                          .Find(filter)
                          .ToListAsync();
            */
        }

        public async Task Create(Product product)
        {
            throw new NotImplementedException();
            
            /*
             * await _context.Products.InsertOneAsync(product);
             * */

        }

        public async Task<bool> Update(Product product)
        {
            throw new NotImplementedException();
            /*
            var updateResult = await _context
                                        .Products
                                        .ReplaceOneAsync(filter: g => g.Id == product.Id, replacement: product);

            return updateResult.IsAcknowledged
                    && updateResult.ModifiedCount > 0;
            */
        }

        public async Task<bool> Delete(string id)
        {
            throw new NotImplementedException();
            /*
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(m => m.Id, id);
            DeleteResult deleteResult = await _context
                                                .Products
                                                .DeleteOneAsync(filter);

            return deleteResult.IsAcknowledged
                && deleteResult.DeletedCount > 0;
            */
        }        
    }
}
