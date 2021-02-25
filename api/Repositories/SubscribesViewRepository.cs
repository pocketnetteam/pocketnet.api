using System.Collections.Generic;
using System.Threading.Tasks;
using api.Extensions;
using api.Repositories.Interfaces;
using api.Services;
using Microsoft.Data.Sqlite;

namespace api.Repositories
{
	public class SubscribesViewRepository : ISubscribesViewRepository
	{
		private readonly CatalogContext _context;

		public SubscribesViewRepository(CatalogContext context)
		{
			_context = context;
		}

		public async Task<List<string>> GetAddressesToByAddressAsync(string address)
		{
			var result = new List<string>();
			
			var commandText = $@"select
 from SubscribesView s where s.address = $address";

			var command = new SqliteCommand(commandText, _context.Connection);

			command.Parameters.AddWithValue("$address", address).SqliteType = SqliteType.Text;

			await using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				result.Add(reader.SafeGetString("address_to"));
			}

			return result;
		}
	}
}