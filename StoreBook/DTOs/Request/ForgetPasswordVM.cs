using System.ComponentModel.DataAnnotations;

namespace StoreBook.DTOs.Request
{
    public class ForgetPasswordVM
    {
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}
