using Catalog.API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<IEnumerable<Getlastcomments>> GetlastcommentsAsync(string address, string lang);
        Task<IEnumerable<Product>> GetProductByNameAsync(string name);
        Task<IEnumerable<Product>> GetProductByCategoryAsync(string categoryName);
                      
        //Task CreateAsync(Product product);
        //Task<bool> UpdateAsync(Product product);
        //Task<bool> DeleteAsync(string id);
    }
}
