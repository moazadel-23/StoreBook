using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using StoreBook.DTOs.Request;
using StoreBook.Repositories.IRepository;
using StoreBook.Models;

namespace StoreBook.Areas.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOtpRepository;

        public AccountsController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IRepository<ApplicationUserOTP> ApplicationUserOtpRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _applicationUserOtpRepository = ApplicationUserOtpRepository;
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var user = new ApplicationUser
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                UserName = registerRequest.UserName
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Code);
                }

                return BadRequest(result.Errors);
            }

            // Send Confirmation Mail
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(confirmEmail), "Accounts", new
            {
                area = "Identity",
                token,
                userId = user.Id
            }, Request.Scheme);
            await _emailSender.SendEmailAsync(registerRequest.Email, "Login Notification",
                $"<h1>confirm your Email by Clicking<a href='{link}'>here</a></h1>");

            return Ok(new
            {
                msg = "Valid Register"
            });
        }
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> confirmEmail(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound(new
                {
                    msg = "Invalide user card"
                });
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest(new
                {
                    msg = "Invalide Token"
                });
            else

                return Ok(new
                {
                    msg = "Confirm email succsed"
                });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(DTOs.Request.LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(loginRequest.UserNameOrEmail);

            if (user is null)
            {
                return BadRequest(new
                {
                    code = "InValid Login",
                    description = "Invalid username or password"
                });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, lockoutOnFailure: true);

          

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return BadRequest(new 
                    {
                        code = "InValid Login",
                        description = "\"Too many attemps, try again after 5 min\""
                    });
                else
                    return BadRequest(new 
                    {
                        code = "InValid Login",
                        description = "\"Invalid User Name / Email OR Password\""
                    });

            }
            return Ok();
        }


        [HttpPost("resendEmailConfirmation")]
        public async Task<IActionResult> resendEmailConfirmation(resendEmailConfirmationRequest resendEmailConfirmationRequest)
        {


            var user = await _userManager.FindByNameAsync(resendEmailConfirmationRequest.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new 
                {
                    code = "User Not Found",
                    description = "No user"
                });
            }

            if (user.EmailConfirmed)
            {
                return Ok(new
                {
                    msg = "Email already confirmed",
                });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(confirmEmail), "Accounts", new
            {
                area = "Identity",
                token,
                userId = user.Id
            }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email!, "Login Notification",
                $"<h1>confirm your Email by Clicking<a href='{link}'>here</a></h1>");


            return Ok(new { msg = "Email confirmation token sent successfully" });

        }

        [HttpPost("forgetPassword")]
        public async Task<IActionResult> forgetPassword(ForgetPasswordVM ForgetPasswordVM)
        {
            var user = await _userManager.FindByNameAsync(ForgetPasswordVM.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(ForgetPasswordVM.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new 
                {
                    code = "User Not Found",
                    description = "No user"
                });
            }

            var otp = new Random().Next(1000, 9999);

            await _applicationUserOtpRepository.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                OTP = otp.ToString(),
                CreateAt = DateTime.Now,
                ValidTo = DateTime.Now.AddDays(1),
                IsValid = true
            });

            var userOTP = await _applicationUserOtpRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var totalOTP = userOTP.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);
            if (totalOTP > 3)
            {
                return BadRequest(new
                {
                    code = "OTP Limit Exceeded",
                    description = "You have exceeded the maximum number of OTP requests allowed in 24 hours."
                });
            }
            await _emailSender.SendEmailAsync(user.Email!, "Reset Your Password",
                $"<h1>Use this OTP:{otp} do not share it</h1>");


            return Ok(new { msg = "OTP sent successfully", userId = user.Id });

        }

        [HttpPost("validateOTP")]
        public async Task<IActionResult> validateOTP(ValidateOTPRequest validateOTPVM)
        {
            var result = await _applicationUserOtpRepository.GetOneAsync(e => e.ApplicationUserId == validateOTPVM.ApplicationUserId && e.OTP == validateOTPVM.OTP && e.IsValid);

            if (result is null)
            {
                return BadRequest(new { msg = "Invalid or expired OTP" });
            }

            return Ok(new { msg = "OTP verified successfully", userId = validateOTPVM.ApplicationUserId });
        }

        [HttpGet("newPassword/{userId}")]
        public IActionResult newPassword(string userId)
        {
            return Ok(new ValidateOTPRequest
            {
                ApplicationUserId = userId
            });
        }
    }
}
