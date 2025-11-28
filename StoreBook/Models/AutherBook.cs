namespace StoreBook.Models
{
    public class AutherBook
    {
        public int Id { get; set; }
        public int AutherId { get; set; }
        public Auther? Auther { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }

    }
}
