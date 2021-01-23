using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Settings;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Threading.Tasks;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        public CatalogContext(ICatalogDatabaseSettings settings)
        {
            connection = new   SqliteConnection(settings.ConnectionString);
            connection.Open();

            cmd = new SqliteCommand("", connection);

            CatalogContextSeed.SeedData(cmd);
        }

        public void Dispose()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open) connection.Close();
        }

        public async Task<SqliteDataReader> CommandExecutor(string SQL)
        {
            cmd.CommandText = SQL;
            return await cmd.ExecuteReaderAsync();
        }
        public async Task<object> CommandExecutorScalar(string SQL)
        {
            cmd.CommandText = SQL;
            return await cmd.ExecuteScalarAsync();
        }
        public async Task CommandExecutorNonQuery(string SQL)
        {
            cmd.CommandText = SQL;
            await cmd.ExecuteNonQueryAsync();
        }



        public SqliteCommand cmd { get; }
        public SqliteConnection connection { get; }

    }
}
