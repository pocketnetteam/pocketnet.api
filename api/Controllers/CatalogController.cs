using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {

            _logger.LogInformation("GetProducts", null);

            var products = await _repository.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("{address:length(34)}", Name = "Getlastcomments")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Getlastcomments>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Getlastcomments>>> GetlastcommentsAsync(string address, string lang, int resultCount)
        {
            _logger.LogInformation($"GetlastcommentsAsync Parameters: {address}, {lang}, {resultCount}" );

            var items = await _repository.GetlastcommentsAsync(address, lang, resultCount);

            if (items == null)
            {
                _logger.LogError($"Getlastcomments No records: {address}, {lang}");
                return NotFound();
            }

            return Ok(items);
        }

        [Route("[action]/{category}")]
        [HttpGet]        
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductByCategory(string category)
        {
            var product = await _repository.GetProductByCategoryAsync(category);
            return Ok(product);
        }

        //[HttpPost]
        //[ProducesResponseType(typeof(Product), (int)HttpStatusCode.Created)]
        //public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        //{
        //    await _repository.CreateAsync(product);

        //    return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        //}

        //[HttpPut]
        //[ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> UpdateProduct([FromBody] Product value)
        //{
        //    return Ok(await _repository.UpdateAsync(value));
        //}

        //[HttpDelete("{id:length(24)}")]
        //[ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> DeleteProductById(string id)
        //{
        //    return Ok(await _repository.DeleteAsync(id));
        //}
    }
}
