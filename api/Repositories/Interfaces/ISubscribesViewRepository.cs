using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.Repositories.Interfaces
{
	public interface ISubscribesViewRepository
	{
		Task<List<string>> GetAddressesToByAddressAsync(string address);
	}
}