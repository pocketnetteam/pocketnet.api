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

        public void Dispose()
        {
       //     _logger.LogInformation("connection.Close", null);

            if (Connection != null && Connection.State == System.Data.ConnectionState.Open) Connection.Close();
        }

        public async Task<SqliteDataReader> CommandExecutor(string SQL)
        {
            Cmd.CommandText = SQL;  
            return await Cmd.ExecuteReaderAsync();
        }
        public async Task<object> CommandExecutorScalar(string SQL)
        {
            Cmd.CommandText = SQL;
            return await Cmd.ExecuteScalarAsync();
        }
        public async Task CommandExecutorNonQuery(string SQL)
        {
            Cmd.CommandText = SQL;
            await Cmd.ExecuteNonQueryAsync();
        }



        public SqliteCommand Cmd { get; }
        public SqliteConnection Connection { get; }

    }
}
