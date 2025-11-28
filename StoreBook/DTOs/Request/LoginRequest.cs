using System.ComponentModel.DataAnnotations;

namespace StoreBook.DTOs.Request
{
    public class LoginRequest
    {
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
