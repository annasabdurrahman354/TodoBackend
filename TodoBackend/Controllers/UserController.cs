using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TodoBackend.Authentication;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace TodoBackend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment hostEnvironment;

        public UserController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            this.userManager = userManager;
            this.hostEnvironment = hostEnvironment;
        }
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null) return NotFound();
            UserModel model = new UserModel();
            model.Username = user.UserName;
            model.Firstname = user.Firstname;
            model.Lastname = user.Lastname;
            model.ImageUrl = String.Format("{0}://{1}{2}/User/{3}/{4}", Request.Scheme, Request.Host, Request.PathBase, username, user.ImageName);
            return Ok(model);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromForm] UserModel userModel)
        {
            if (username != userModel.Username)
            {
                return BadRequest();
            }

            var user = await userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            if (userModel.ImageFile != null)
            {
                DeleteImage(user.UserName, user.ImageName);
                user.ImageName = await SaveImage(user.UserName, userModel.ImageFile);
            }

            user.Firstname = userModel.Firstname;
            user.Lastname = userModel.Lastname;
            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok();
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [NonAction]
        public async Task<string> SaveImage(string username, IFormFile imageFile)
        {
            var imageName = username + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(imageFile.FileName);
            var imagePath = Path.Combine(hostEnvironment.WebRootPath, "User", username, imageName);
            if (!Directory.Exists(Path.GetDirectoryName(imagePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
            }
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return imageName;
        }

        [NonAction]
        public void DeleteImage(string username, string imageName)
        {
            var imagePath = Path.Combine(hostEnvironment.WebRootPath, "User", username, imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }
    }
}
