using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoBackend.Models;
using TodoBackend.Repositories;

namespace TodoBackend.Controllers
{
    [Route("api/blog")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository postRepository;

        public PostController(IPostRepository postRepository)
        {
            this.postRepository = postRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostModel>>> GetAll()
        {
            try
            {
                return (await postRepository.GetAll()).ToList();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet]
        [Route("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostModel>>> GetUserPosts(string userId)
        {
            try
            {
                return (await postRepository.GetUserPosts(userId)).ToList();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PostModel>> Get(int id)
        {
            try
            {
                var result = await postRepository.Get(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PostModel>> Add([FromBody] PostModel post)
        {
            try
            {
                if (post == null)
                    return BadRequest();

                var newPost = await postRepository.Add(post);

                return CreatedAtAction(nameof(Get),
                    new { id = newPost.Id }, newPost);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new post record");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PostModel>> Update(int id, [FromBody] PostModel post)
        {
            try
            {
                if (id != post.Id)
                    return BadRequest("Post's Id mismatch");

                var updatedPost = await postRepository.Get(id);

                if (updatedPost == null)
                    return NotFound($"Post with Id = {id} not found");

                return await postRepository.Update(post);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<PostModel>> Delete(int id)
        {
            try
            {
                var postToDelete = await postRepository.Get(id);

                if (postToDelete == null)
                {
                    return NotFound($"Post with Id = {id} not found");
                }

                return await postRepository.Delete(id);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        [HttpPost]
        [Route("upload-image/{username}")]
        public async Task<ActionResult<IFormFile>> UploadImage(string username, IFormFile file)
        { 
            string imageName = await postRepository.UploadImage(username, file);
            if(imageName == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error uploading image");
            }
            return Ok(new { location = String.Format("{0}://{1}{2}/Post/{3}/Image/{4}", Request.Scheme, Request.Host, Request.PathBase, username, imageName) });
        }
    }
}
