using System.ComponentModel.DataAnnotations;

namespace StoreBook.DTOs.Request
{
    public class valideOTPRequest
    {
        [Required]
        public string OTP { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
