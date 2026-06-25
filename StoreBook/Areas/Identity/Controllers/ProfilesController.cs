using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StoreBook.Models;
using Mapster;
using StoreBook.DTOs.Request;

namespace StoreBook.Areas.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilesController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // =============================
        // GET: api/Profile/index
        // =============================
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound(new ErrorModelRequest
                {
                    code = "404",
                    description = "User not found"
                });

            var userVM = user.Adapt<ApplicationUserResponse>();

            return Ok(userVM);
        }

        // =============================
        // POST: api/Profile/UpdateProfile
        // =============================
        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserResponse model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound(new ErrorModelRequest
                {
                    code = "404",
                    description = "User not found"
                });

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return BadRequest(new ErrorModelRequest
                {
                    code = "400",
                    description = "FullName is required"
                });
            }

            var names = model.FullName.Split(' ');

            user.FirstName = names.First();
            user.LastName = names.Length > 1 ? names[1] : "";
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorModelRequest
                {
                    code = "400",
                    description = string.Join(",", result.Errors.Select(e => e.Description))
                });
            }

            return Ok(new
            {
                message = "Profile updated successfully"
            });
        }

        // =============================
        // POST: api/Profile/UpdatePassword
        // =============================
        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(ApplicationUserResponse model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
                return NotFound(new ErrorModelRequest
                {
                    code = "404",
                    description = "User not found"
                });

            if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest(new ErrorModelRequest
                {
                    code = "400",
                    description = "CurrentPassword and NewPassword are required"
                });
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorModelRequest
                {
                    code = "400",
                    description = string.Join(",", result.Errors.Select(e => e.Description))
                });
            }

            return Ok(new
            {
                message = "Password updated successfully"
            });
        }
    }
}
