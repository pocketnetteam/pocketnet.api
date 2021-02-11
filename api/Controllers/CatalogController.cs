using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using api.DTOs;
using api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("Getlastcomments")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<Comment>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Comment>>> GetLastCommentsAsync([Required, MaxLength(34)] string address, string lang, [DefaultValue(100)] int resultCount)
        {
            _logger.LogInformation($"GetLastCommentsAsync Parameters: {address}, {lang}, {resultCount}");

            var items = await _repository.GetLastCommentsAsync(address, lang, resultCount);

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
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsAsync(string postid, string parentid, string address, string comment_ids, [DefaultValue(100)] int resultCount)
        {
           // _logger.LogInformation($"GetCommentsAsync Parameters: {postid}, {parentid}, {address}");

            var items = await _repository.GetCommentsAsync(postid, parentid, address, comment_ids, resultCount);

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
        public async Task<ActionResult<IEnumerable<Score>>> GetPageScoresAsync([DefaultValue("")]string tx_ids, [DefaultValue("")]string address, [DefaultValue("")] string comment_ids, [DefaultValue(100)] int resultCount)
        {
            //TODO cache
            var items = await _repository.GetPageScoresAsync(tx_ids, address, comment_ids, resultCount);

            if (items == null)
            {
                _logger.LogError($"GetPageScoresAsync No records: {tx_ids}, {address}, {comment_ids}");
                return NotFound();
            }

            return Ok(items);
        }
    }
}
