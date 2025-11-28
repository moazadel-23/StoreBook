using StoreBook.Models;

namespace StoreBook.DTOs.Request
{
    public class filterBookRequest
    {
        public string Titel { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Rate { get; set; }
        public string MainImg { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int AutherId { get; set; }
    }
}
