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
using TodoBackend.Template;

namespace TodoBackend.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly EmailTemplate emailTemplate = new EmailTemplate();
        private const string FRONT_END_URL = "http://localhost:3000/";
        

        public AuthenticateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
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

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    user,
                    expiration = token.ValidTo
                });
            }
            else if (user != null && await userManager.CheckPasswordAsync(user, model.Password) && await userManager.IsEmailConfirmedAsync(user) == false)
            {
                return Unauthorized("User account has not been confirmed!");
            }
            return Unauthorized("Something went wrong");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Conflict("User already exists!");

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
            try
            {
                await SendVerificationEmail(user.UserName);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return Ok("User created successfully!");
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return Conflict("User already exists!");

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);

            if (!await roleManager.RoleExistsAsync(UserRolesModel.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRolesModel.Admin));
            if (!await roleManager.RoleExistsAsync(UserRolesModel.User))
                await roleManager.CreateAsync(new IdentityRole(UserRolesModel.User));

            if (await roleManager.RoleExistsAsync(UserRolesModel.Admin))
            {
                await userManager.AddToRoleAsync(user, UserRolesModel.Admin);
            }

            return Ok("User created successfully!");
        }

        [HttpGet]
        [Route("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found!");
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == true)
            {
                return Conflict("Users email has been confirmed!");
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
                return NotFound("User not found!");
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == true)
            {
                return Ok(true);
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == false)
            {
                return Ok(false);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong!");
            }
        }

        [HttpGet]
        [Route("send-verification-email")]
        public async Task<IActionResult> SendVerificationEmail([FromQuery] string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound("User not found!");
            }
            else if (user != null && await userManager.IsEmailConfirmedAsync(user) == true)
            {
                return Conflict("Users email has been confirmed!");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("VerifyEmail", "Authenticate", new { email = user.Email, token}, Request.Scheme);
            EmailSender emailHelper = new EmailSender();
            try
            {
                await emailHelper.SendEmailAsync(user.Email, "Verify your email!", emailTemplate.EmailVerification(confirmationLink));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
            return Ok(confirmationLink);
        }

        [HttpGet]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found!");
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var confirmationLink = FRONT_END_URL + "reset-password/" + email + "/" + token.Replace("/", "%2F");
            EmailSender emailHelper = new EmailSender();
            try
            {
                await emailHelper.SendEmailAsync(user.Email, "Do you forget your password?", emailTemplate.ForgetPassword(confirmationLink));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            return Ok(new {email = user.Email, token = token, resetPasswordLink = confirmationLink});
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel, [FromQuery] string email, [FromQuery] string token)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found!");
            if (await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token))
            {
                var resetPassResult = await userManager.ResetPasswordAsync(user, token, resetPasswordModel.NewPassword);
                if (!resetPassResult.Succeeded)
                {
                    StatusCode(StatusCodes.Status500InternalServerError, resetPassResult.Errors);
                }
            }
            else
            {
                BadRequest("Token is invalid");
            }
            return Ok("Password changed succesfully");
        }







        [HttpPost]
        [Route("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel changeEmailModel)
        {
            var user = await userManager.FindByNameAsync(changeEmailModel.Username);
            if (user == null)
            {
                return NotFound("User not found!");
            }
            if (!await userManager.CheckPasswordAsync(user, changeEmailModel.Password))
            {
                return BadRequest("Wrong paswword!");
            }
            if (await userManager.FindByEmailAsync(changeEmailModel.NewEmail) != null)
            {
                return Conflict("New email has been used!");
            }
            var token = await userManager.GenerateChangeEmailTokenAsync(user, changeEmailModel.NewEmail);
            var changePasswordResult = await userManager.ChangeEmailAsync(user, changeEmailModel.NewEmail, token);
            if (!changePasswordResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, changePasswordResult.Errors);
            }
            return Ok("Email changed succesfully");
        }

        [HttpPost]
        [Route("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserModel updateUserModel)
        {
            var user = await userManager.FindByNameAsync(updateUserModel.Username);
            if (user == null)
            {
                return NotFound("User not found!");
            }
            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, updateUserResult.Errors);
            }
            return Ok("Email changed succesfully");
        }
    }
}
