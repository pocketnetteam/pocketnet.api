using System;
using System.Data;
using System.Threading.Tasks;
using api.Settings;
using Microsoft.Data.Sqlite;

namespace api.Services
{
    public class CatalogContext : IAsyncDisposable
    {
        public SqliteConnection Connection { get; }

        public CatalogContext(ICatalogDatabaseSettings settings)
        {
            Connection = new   SqliteConnection(settings.ConnectionString);
            Connection.Open();
            
            // _logger.LogInformation("connection.Open", null);

            // Cmd = new SqliteCommand("", Connection)
            // {
            //     CommandText = @"PRAGMA journal_mode = 'wal'"
            // };

            // Enable write-ahead logging
            //Cmd.ExecuteNonQuery();

            //CatalogContextSeed.SeedData(Cmd);
        }

        public async ValueTask DisposeAsync()
        {
            if (Connection != null && Connection.State == ConnectionState.Open)
            {
                await Connection.CloseAsync();
            }
        }
    }
}
