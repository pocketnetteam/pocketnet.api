using System;
using DynaCache.Attributes;

namespace api.Services
{
	public class TestCacheableService
	{
		[CacheableMethod(60)]
		public virtual DateTime GetCurrentDateTime(int seed)
		{
			return DateTime.Now;
		}
	}
}