using Catalog.API.Entities;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace Catalog.API.Data.Interfaces
{
    public interface ICatalogContext
    {
        SqliteCommand Cmd { get; }
        SqliteConnection Connection { get; }
        public Task<SqliteDataReader> CommandExecutor(string SQL);
        public Task<object> CommandExecutorScalar(string SQL);
        public Task CommandExecutorNonQuery(string SQL);
    }
}
