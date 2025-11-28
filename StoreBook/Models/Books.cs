namespace StoreBook.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Rate { get; set; }
        public string Img { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

    }
}
