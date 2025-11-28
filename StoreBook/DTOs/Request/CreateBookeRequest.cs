using System.ComponentModel.DataAnnotations;

public class CreateBookeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    [Required]
    public IFormFile MainImg { get; set; } = null!;

    public decimal Price { get; set; }
    public bool Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Rate { get; set; }

    public string ImageName { get; set; } = string.Empty; 

    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
}
