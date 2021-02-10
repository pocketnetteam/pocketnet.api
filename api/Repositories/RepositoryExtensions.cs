using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repositories
{
    public static class SqlDataReaderExtensions
    {
        public static int SafeGetInt32(this DbDataReader reader,
                                       string columnName, int defaultValue = 0)
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetInt32(ordinal);
            }
            else
            {
                return defaultValue;
            }
        }
        public static string SafeGetString(this DbDataReader reader,
                               string columnName, string defaultValue = "")
        {
            int ordinal = reader.GetOrdinal(columnName);

            if (!reader.IsDBNull(ordinal))
            {
                return reader.GetString(ordinal);
            }
            else
            {
                return defaultValue;
            }
        }
    }


    public static class StringExtensions
    {
        public static List<string> FromJArray(this string jarray)
        {
            List<string> res = new List<string>();
            if (jarray != "")
            {
                try
                {
                    res = JArray.Parse(jarray).ToObject<List<string>>();
                    res.ForEach(x => x.Replace("'", ""));
                }
                catch { }
            }

            return res;
        }
    }

}
