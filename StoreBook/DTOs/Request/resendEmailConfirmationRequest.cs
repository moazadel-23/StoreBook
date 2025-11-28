using System.ComponentModel.DataAnnotations;

namespace StoreBook.DTOs.Request
{
    public class resendEmailConfirmationRequest
    {

            [Required]
            public string UserNameOrEmail { get; set; } = string.Empty;
        
    }
}
