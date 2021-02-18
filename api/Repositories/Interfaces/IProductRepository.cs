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
        Task<IEnumerable<UserProfile>> GetUserProfileAsync(string addresses, int shortForm = 1, int option = 0);
    }
}
