using System;
using System.Net;
using api.Services;
using DynaCache.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
	[Microsoft.AspNetCore.Components.Route("api/v1/[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly TestCacheableService _testCacheableService;
		private readonly IDynaCacheService _dynaCacheService;

		public TestController(TestCacheableService testCacheableService, IDynaCacheService dynaCacheService)
		{
			_testCacheableService = testCacheableService;
			_dynaCacheService = dynaCacheService;
		}

		[HttpGet("CurrentDate")]
		[ProducesResponseType(typeof(DateTime), (int)HttpStatusCode.OK)]
		public ActionResult<DateTime> CurrentDate(int seed)
		{
			var date = _testCacheableService.GetCurrentDateTime(seed);

			return Ok(date);
		}

		[HttpPost("ResetCache")]
		public ActionResult ResetCache()
		{
			_dynaCacheService.ClearCache();

			return Ok();
		}
	}
}