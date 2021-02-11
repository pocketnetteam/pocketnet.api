using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace api.Extensions
{
    public static class StringExtensions
    {
        public static List<string> FromJArray(this string jArray)
        {
            var res = new List<string>();
            if (jArray == "")
            {
                return res;
            }

            try
            {
                res = JArray.Parse(jArray).ToObject<List<string>>()?
                    .Select(x => x.Replace("'", ""))
                    .ToList();
            }
            catch
            {
                //ignore
            }

            return res;
        }
    }
}
