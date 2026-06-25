namespace StoreBook.Models
{
    public class Cart
    {
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; }
    }
}
