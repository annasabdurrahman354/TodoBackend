using TodoBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Services;
using TodoBackend.Authentication;

namespace TodoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        const string FRONT_END_URL = "http://localhost:3000/";

        public AuthenticateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password) && await userManager.IsEmailConfirmedAsync(user) )
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    username = user.UserName,
                    firstname = user.Firstname,
                    lastname = user.Lastname,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            else if (user != null && await userManager.CheckPasswordAsync(user, model.Password) && await userManager.IsEmailConfirmedAsync(user) == false)
            {
                //Redirect("localhost:3120/confirm-email")
                return StatusCode(StatusCodes.Status401Unauthorized, new ResponseModel { Status = "Error", Message = "User account has not been confirmed!" });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                //Country = model.Country,
                //PhoneNumber = model.PhoneNumber,
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("VerifyEmail", "Authenticate", new { email = user.Email, token }, Request.Scheme);
            EmailSender emailHelper = new EmailSender();
            try
            {
                await emailHelper.SendEmailAsync(user.Email, "Verify Your Email",
                    "<html>" +
                    "<body> <h3>Verify your email by clicking this link: </h3> <p>" + confirmationLink + "</p> </body>" +
                    "</html>");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = ex.Message });
            }

            return Ok(new ResponseModel { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);

            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return Ok(new ResponseModel { Status = "Success", Message = "User created successfully!" });
        }

        [HttpGet]
        [Route("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == true)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User's email has been confirmed!" });
            }

            await userManager.ConfirmEmailAsync(user, token);
            return Redirect(FRONT_END_URL + "login");
        }

        [HttpGet]
        [Route("check-email-verified")]
        public async Task<IActionResult> CheckEmailVerified([FromQuery] string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == true)
            {
                return Ok(new ResponseModel { Status = "Success", Message = 1 });
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == false)
            {
                return Ok(new ResponseModel { Status = "Success", Message = 0 });
            }
            else
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "An error ocurred!" });
            }
        }

        [HttpGet]
        [Route("send-verification-email")]
        public async Task<IActionResult> SendVerificationEmail([FromQuery] string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == true)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User's email has been confirmed!" });
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("VerifyEmail", "Authenticate", new { email = user.Email, token}, Request.Scheme);
            EmailSender emailHelper = new EmailSender();
            try
            {
                await emailHelper.SendEmailAsync(user.Email, "Verify Your Email", 
                    "<html>" +
                    "<body> <h3>Verify your email by clicking this link: </h3> <p>" + confirmationLink + "</p> </body>" +
                    "</html>");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = ex.Message });
            }
            
            return Ok(new ResponseModel { Status = "Success", Message = confirmationLink });
        }

        [HttpGet]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var confirmationLink = FRONT_END_URL + "reset-password/" + email + "/" + token;
            EmailSender emailHelper = new EmailSender();
            try
            {
                await emailHelper.SendEmailAsync(user.Email, "Forgot Password Email",
                    "<html>" +
                    "<body> <h3>Change your account password by clicking this link: </h3> <p>" + confirmationLink + "</p> </body>" +
                    "</html>");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = ex.Message } );
            }
            return Ok(new ResponseModel { Status = "Success", Message = new { email = user.Email, token = token, resetPasswordLink = confirmationLink} });
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel, [FromQuery] string email, [FromQuery] string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            if (await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token))
            {
                var resetPassResult = await userManager.ResetPasswordAsync(user, token, resetPasswordModel.NewPassword);
                if (!resetPassResult.Succeeded)
                {
                    StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = resetPassResult.Errors });
                }
            }
            else
            {
                StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "Token is invalid" });
            }
            return Ok(new ResponseModel { Status = "Success", Message = "Password changed succesfully" });
        }

        [HttpPost]
        [Route("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel changeEmailModel)
        {
            var user = await userManager.FindByNameAsync(changeEmailModel.Username);
            if (user == null)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            }
            if (!await userManager.CheckPasswordAsync(user, changeEmailModel.Password))
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "Wrong paswword!" });
            }
            if (await userManager.FindByEmailAsync(changeEmailModel.NewEmail) != null)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "New email has been used!" });
            }
            var token = await userManager.GenerateChangeEmailTokenAsync(user, changeEmailModel.NewEmail);
            var changePasswordResult = await userManager.ChangeEmailAsync(user, changeEmailModel.NewEmail, token);
            if (!changePasswordResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = changePasswordResult.Errors });
            }
            return Ok(new ResponseModel { Status = "Success", Message = "Email changed succesfully" });
        }

        [HttpPost]
        [Route("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel updateUserModel)
        {
            var user = await userManager.FindByNameAsync(updateUserModel.Username);
            if (user == null)
            {
                return BadRequest(new ResponseModel { Status = "Error", Message = "User not found!" });
            }
            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = updateUserResult.Errors });
            }
            return Ok(new ResponseModel { Status = "Success", Message = "Email changed succesfully" });


            if (updateUserModel.Username == null)
            {
                return NotFound();
            }
            var userToUpdate = await userManager.FindByNameAsync(updateUserModel.Username);

           
        }
    }
}
