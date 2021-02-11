using System;
using System.Data;
using System.Threading.Tasks;
using api.Services.Interfaces;
using api.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace api.Services
{
    public class CatalogContext : ICatalogContext, IAsyncDisposable
    {
        private readonly ILogger<CatalogContext> _logger;

        public SqliteCommand Cmd { get; }
        public SqliteConnection Connection { get; }

        public CatalogContext(ICatalogDatabaseSettings settings, ILogger<CatalogContext> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Connection = new   SqliteConnection(settings.ConnectionString);
            Connection.Open();
            
            // _logger.LogInformation("connection.Open", null);

            Cmd = new SqliteCommand("", Connection)
            {
                CommandText = @"PRAGMA journal_mode = 'wal'"
            };

            // Enable write-ahead logging
            Cmd.ExecuteNonQuery();

            //CatalogContextSeed.SeedData(Cmd);
        }

        public async Task<SqliteDataReader> CommandExecutor(string sql)
        {
            Cmd.CommandText = sql;  
            return await Cmd.ExecuteReaderAsync();
        }

        public async Task<object> CommandExecutorScalar(string sql)
        {
            Cmd.CommandText = sql;
            return await Cmd.ExecuteScalarAsync();
        }

        public async Task CommandExecutorNonQuery(string sql)
        {
            Cmd.CommandText = sql;
            await Cmd.ExecuteNonQueryAsync();
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
