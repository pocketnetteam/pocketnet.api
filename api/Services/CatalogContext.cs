using Catalog.API.Data.Interfaces;
using Catalog.API.Entities;
using Catalog.API.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext, IDisposable
    {
        private readonly ILogger<CatalogContext> _logger;

        public CatalogContext(ICatalogDatabaseSettings settings, ILogger<CatalogContext> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            //Connection = new SQLiteConnection(settings.ConnectionString);
            //Connection.Open();

            for (int i = 0; i < 10; i++)
            {
                Connections[i] = new SQLiteConnection(settings.ConnectionString);
                Connections[i].Open();
            }

            // _logger.LogInformation("connection.Open", null);

            //Cmd = new SqliteCommand("", Connection);

            // Enable write-ahead logging
            // Cmd.CommandText =   @"PRAGMA journal_mode = 'wal'";
            // Cmd.ExecuteNonQuery();

            // Cmd.CommandText =   @"PRAGMA TEMP_STORE = 'MEMORY'";
            // Cmd.ExecuteNonQuery();

            // Cmd.CommandText = @"PRAGMA LOCKING_MODE = 'EXCLUSIVE'";
            // Cmd.ExecuteNonQuery();


            //CatalogContextSeed.SeedData(Cmd);

            using (var conn = new SQLiteConnection("Data Source=:memory:"))
            {
                var cmd1 = conn.CreateCommand();
                cmd1.CommandText = "PRAGMA compile_options";
                conn.Open();
                using (var reader = cmd1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetValue(0));
                    }
                }
            }
        }

        public void Dispose()
        {
            //     _logger.LogInformation("connection.Close", null);

            if (Connection != null && Connection.State == System.Data.ConnectionState.Open) Connection.Close();
        }

        public async Task<DbDataReader> CommandExecutor(SQLiteCommand Cmd)
        {
            return await Cmd.ExecuteReaderAsync();
        }
        
        public async Task<DbDataReader> CommandExecutor(string SQL)
        {
            var cmd = new SQLiteCommand(SQL, Connection);
            return await cmd.ExecuteReaderAsync();
        }

        public async Task<object> CommandExecutorScalar(string SQL)
        {
            var cmd = new SQLiteCommand(SQL, Connection);
            return await cmd.ExecuteScalarAsync();
        }

        public async Task CommandExecutorNonQuery(string SQL)
        {
            var cmd = new SQLiteCommand(SQL, Connection);
            await cmd.ExecuteNonQueryAsync();
        }


        public SQLiteCommand Cmd { get; }
        public SQLiteConnection Connection { get; }

        public SQLiteConnection[] Connections { get; } = new SQLiteConnection[10];
    }
}