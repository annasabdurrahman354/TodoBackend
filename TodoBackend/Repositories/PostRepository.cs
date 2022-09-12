using System.Collections.Generic;
using System.Threading.Tasks;
using TodoBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TodoBackend.Authentication;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;

namespace TodoBackend.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IWebHostEnvironment hostEnvironment;

        public PostRepository(ApplicationDbContext appDbContext, IWebHostEnvironment hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;
            this.appDbContext = appDbContext;
        }

        public async Task<IEnumerable<PostModel>> GetAll()
        {
            return await appDbContext.Post.ToListAsync();
        }

        public async Task<PostModel> Get(int id)
        {
            return await appDbContext.Post.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<PostModel> Add(PostModel post)
        {
            var result = await appDbContext.Post.AddAsync(post);
            await appDbContext.SaveChangesAsync();
            return result.Entity;
        }
        public async Task<PostModel> Update(PostModel post)
        {
            var result = await appDbContext.Post.FirstOrDefaultAsync(e => e.Id == post.Id);

            if (result != null)
            {
                result.Title = post.Title;
                result.Description = post.Description;
                result.ThumbnailUrl = post.ThumbnailUrl;
                result.Content = post.Content;
                await appDbContext.SaveChangesAsync();

                return result;
            }

            return null;
        }

        public async Task<PostModel> Delete(int id)
        {
            var result = await appDbContext.Post
                .FirstOrDefaultAsync(e => e.Id == id);
            if (result != null)
            {
                appDbContext.Post.Remove(result);
                await appDbContext.SaveChangesAsync();
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<PostModel>> GetUserPosts(string id)
        {
            return await appDbContext.Post.Where(e => e.UserId == id).ToListAsync(); ;
        }

        public async Task<string> UploadImage(string username, IFormFile file)
        {
            string wwwRootPath = hostEnvironment.WebRootPath;
            string fileName = new String(Path.GetFileNameWithoutExtension(file.FileName).Take(10).ToArray()).Replace(' ', '-');
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, "Post", username, "Image", fileName);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return fileName;
        }
    }
}
