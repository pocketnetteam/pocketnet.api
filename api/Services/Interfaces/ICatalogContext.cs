using Catalog.API.Entities;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace Catalog.API.Data.Interfaces
{
    public interface ICatalogContext
    {
        SqliteCommand Cmd { get; }
        SqliteConnection Connection { get; }
        public Task<SqliteDataReader> CommandExecutor(string sql);
        public Task<object> CommandExecutorScalar(string sql);
        public Task CommandExecutorNonQuery(string sql);
    }
}
