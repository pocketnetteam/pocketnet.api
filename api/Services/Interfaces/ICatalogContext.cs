using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace api.Services.Interfaces
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
