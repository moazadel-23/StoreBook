using System.ComponentModel.DataAnnotations;

namespace StoreBook.Models
{
    public class Category
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Status { get; set; }

    }
}
