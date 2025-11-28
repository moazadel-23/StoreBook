using System.ComponentModel.DataAnnotations;

namespace StoreBook.Models
{
    public class Auther
    {
        [Required]
        public int AutherId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public double salary { get; set; }

    }
}
