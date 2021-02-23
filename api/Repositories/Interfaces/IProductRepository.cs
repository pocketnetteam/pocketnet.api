using System.Collections.Generic;
using System.Threading.Tasks;
using api.DTOs;

namespace api.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Comment>> GetCommentsAsync(string postId, string parentId, string address, string commentIds, int resultCount = 100);
        Task<IEnumerable<Comment>> GetLastCommentsAsync(string address, string lang, int resultCount = 100);
        Task<IEnumerable<Score>> GetPageScoresAsync(string txIds, string address, string commentIds, int resultCount = 100);
        Task<IEnumerable<UserProfile>> GetUserProfileAsync(string addresses, bool shortForm = true, int option = 0);
        Task<IEnumerable<Tag>> GetTagsAsync(string address, int count, int block, string lang);
        Task<IEnumerable<UserAddress>> GetUserAddressAsync(string name, int count);
        Task<IEnumerable<Content>> GetContentsAsync(string address, string lang, int count);
        Task<IEnumerable<PostData>> GetRawTransactionWithMessageByIdAsync(string txIds, string address);
        Task<Search> SearchAsync(string search_string, string type, string address, int blockNumber, int resultStart, int resultCount);
    }
}
