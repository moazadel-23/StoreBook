namespace StoreBook.Models
{
    public class Cart
    {
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int ApplicationUserId { get; set; }
       // public ApplicationUser user { get; set; }
    }
}
