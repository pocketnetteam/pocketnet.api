using Microsoft.Data.Sqlite;

namespace api.Extensions
{
	public static class SqlDataReaderExtensions
	{
		public static int SafeGetInt32(this SqliteDataReader reader, string columnName, int defaultValue = 0)
		{
			var ordinal = reader.GetOrdinal(columnName);

			return !reader.IsDBNull(ordinal) 
				? reader.GetInt32(ordinal) 
				: defaultValue;
		}
		public static bool SafeGetBool(this SqliteDataReader reader, string columnName, bool defaultValue = false)
		{
			var ordinal = reader.GetOrdinal(columnName);

			return !reader.IsDBNull(ordinal)
				? reader.GetBoolean(ordinal)
				: defaultValue;
		}
		public static string SafeGetString(this SqliteDataReader reader, string columnName, string defaultValue ="")
		{
			var ordinal = reader.GetOrdinal(columnName);

			return !reader.IsDBNull(ordinal) 
				? reader.GetString(ordinal) 
				: defaultValue;
		}
	}
}