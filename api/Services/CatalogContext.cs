using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext, IDisposable
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

            Cmd = new SqliteCommand("", Connection);

            // Enable write-ahead logging
            Cmd.CommandText =   @"PRAGMA journal_mode = 'wal'";
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

        public void Dispose()
        {
            //     _logger.LogInformation("connection.Close", null);

            if (Connection != null && Connection.State == ConnectionState.Open) Connection.Close();
        }
    }
}
