using Catalog.API.Entities;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Catalog.API.Data.Interfaces
{
    public interface ICatalogContext
    {
        SQLiteCommand Cmd { get; }
        SQLiteConnection Connection { get; }
        public SQLiteConnection[] Connections { get; }
        public Task<DbDataReader> CommandExecutor(SQLiteCommand Cmd);
        public Task<DbDataReader> CommandExecutor(string SQL);
        public Task<object> CommandExecutorScalar(string SQL);
        public Task CommandExecutorNonQuery(string SQL);
    }
}
