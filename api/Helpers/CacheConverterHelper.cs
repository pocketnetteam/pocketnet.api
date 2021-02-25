using System.Collections.Generic;

namespace api.Helpers
{
	public static class CacheConverterHelper
	{
		public static string ConvertList<T>(List<T> list)
		{
			return string.Join(",", list);
		}
	}
}