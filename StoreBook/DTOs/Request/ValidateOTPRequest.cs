using System.ComponentModel.DataAnnotations;

namespace StoreBook.DTOs.Request
{
    public class ValidateOTPRequest
    {
        [Required]
        public string OTP { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;
    }
}