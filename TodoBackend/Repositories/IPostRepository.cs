using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoBackend.Models;

namespace TodoBackend.Repositories
{
    public interface IPostRepository
    {
        Task<IEnumerable<PostModel>> GetAll();
        Task<IEnumerable<PostModel>> GetUserPosts(string userId);
        Task<PostModel> Get(int id);
        Task<PostModel> Add(PostModel post);
        Task<PostModel> Update(PostModel post);
        Task<PostModel> Delete(int id);
        Task<string> UploadImage(string username, IFormFile file);
    }
}
