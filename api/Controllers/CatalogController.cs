using api.DTOs;
using Catalog.API.Entities;
using Catalog.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        private IMemoryCache _cache;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger, IMemoryCache memoryCache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        [HttpGet("Getlastcomments")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Comment>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> GetlastcommentsAsync([Required, MaxLength(34)] string address, string lang, [DefaultValue(100)] int resultCount)
        {
            _logger.LogInformation($"GetlastcommentsAsync Parameters: {address}, {lang}, {resultCount}");

            var items = await _repository.GetlastcommentsAsync(address, lang, resultCount);

            if (items == null)
            {
                _logger.LogError($"Getlastcomments No records: {address}, {lang}");
                return NotFound();
            }

            return Ok(items);
        }

        [HttpGet("Getcomments")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Comment>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> GetcommentsAsync(string postid, string parentid, string address, string comment_ids, [DefaultValue(100)] int resultCount)
        {
           // _logger.LogInformation($"GetcommentsAsync Parameters: {postid}, {parentid}, {address}");

            var items = await _repository.GetcommentsAsync(postid, parentid, address, comment_ids, resultCount);

            if (items == null)
            {
                _logger.LogError($"Getcomments No records: {postid}, {parentid}, {address}");
                return NotFound();
            }

            return Ok(items);
        }
        [HttpGet("Getpagescores")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Score>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Score>>> GetpagescoresAsync(string tx_ids, string address, string comment_ids, [DefaultValue(100)] int resultCount)
        {
            if (string.IsNullOrEmpty(tx_ids)) { tx_ids = ""; }
            if (string.IsNullOrEmpty(address)) { address = ""; }
            if (string.IsNullOrEmpty(comment_ids)) { comment_ids = ""; }

            string key = "Getpagescores" + tx_ids + address + comment_ids + resultCount.ToString();

            //_logger.LogInformation($"GetpagescoresAsync Parameters Start: {tx_ids}, {address}, {comment_ids}");

            IEnumerable<Score> items;
            if (!_cache.TryGetValue(key, out items))
            {
                items = await _repository.GetpagescoresAsync(tx_ids, address, comment_ids, resultCount);

                //_logger.LogInformation($"GetpagescoresAsync write to _cache " + key);

                _cache.Set(key, items, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(1)));
            }
            else
            {
               // _logger.LogInformation($"GetpagescoresAsync read from _cache " + key);
            }

           // _logger.LogInformation($"GetpagescoresAsync Stop");

            if (items == null)
            {
                _logger.LogError($"GetpagescoresAsync No records: {tx_ids}, {address}, {comment_ids}");
                return NotFound();
            }

            return Ok(items);
        }

    }
}
