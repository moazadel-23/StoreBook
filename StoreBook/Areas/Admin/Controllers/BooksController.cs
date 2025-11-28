using Azure.Core;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreBook.DTOs.Request;
using StoreBook.Models;
using StoreBook.Repositories.IRepository;
using System.Threading.Tasks;

namespace StoreBook.Areas
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        public IRepository<Book> _bookRepository { get; }
        public IRepository<Brand> _brandRepository { get; }
        public IRepository<Category> _categoryRepository { get; }
        public ApplicationDbContext _DbContext { get; }

        public BooksController(IRepository<Book> BookRepository, IRepository<Brand> BrandRepository, IRepository<Category> CategoryRepository, ApplicationDbContext applicationDbContext)
        {
            _bookRepository = BookRepository;
            _brandRepository = BrandRepository;
            _categoryRepository = CategoryRepository;
            _DbContext = applicationDbContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] filterBookRequest filterBookRequest, CancellationToken cancellationToken, int page)
        {
            if (page < 1)
                page = 1;

            var book = await _bookRepository.GetAsync(include: new System.Linq.Expressions.Expression<Func<Book, object>>[]
            {
        e => e.Category!,
        e => e.Brand!
            }, tracked: false);

            // فلترة العنوان
            if (!string.IsNullOrWhiteSpace(filterBookRequest.Titel))
            {
                string TitelTrim = filterBookRequest.Titel.Trim();
                book = book.Where(e => e.Title.Contains(TitelTrim, StringComparison.OrdinalIgnoreCase)).ToList();
                filterBookRequest.Titel = TitelTrim;
            }

            // Pagination
            int totalItems = book.Count();
            const int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var bagenation = new BagenationRequest
            {
                TotalPages = totalPages,
                CurrentPage = page
            };

            var booksOnPage = book.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                Books = booksOnPage,
                Bagenation = bagenation,
                filter = filterBookRequest
            });
        }
        [HttpPost("")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateBookeRequest createBookeRequest, CancellationToken cancellationToken)
        {
            await using var transaction = await _DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {

                Book book = Request.Adapt<Book>();
                Book book1 = createBookeRequest.Adapt<Book>();

                // Ensure directories exist
                string root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string imagesRoot = Path.Combine(root, "images");
                string productImagesRoot = Path.Combine(imagesRoot, "product_images");

                Directory.CreateDirectory(imagesRoot);
                Directory.CreateDirectory(productImagesRoot);

                if (createBookeRequest.MainImg != null && createBookeRequest.MainImg.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBookeRequest.MainImg!.FileName);
                    string filePath = Path.Combine(productImagesRoot, fileName);

                    await using (var stream = System.IO.File.Create(filePath))
                    {
                        await createBookeRequest.MainImg.CopyToAsync(stream, cancellationToken);
                    }

                    createBookeRequest.ImageName = fileName;
                }
                var bookCreated = await _bookRepository.AddAsync(book, cancellationToken);
                await _bookRepository.CommitAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return Ok(new
                {
                    success = true,
                    message = "Product created successfully",
                    BookId = bookCreated.BookId
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Product InValid",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Edit( int id, [FromForm] Book book, IFormFile? img,CancellationToken cancellationToken)
        {
            var bookInDb = await _bookRepository.GetOneAsync(e => e.BookId == id, traced: false, cancellationToken: cancellationToken);

            if (img is not null)
            {
                if (img.Length > 0)
                {
                  
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); 
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", bookInDb!.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    book.Img = fileName;
                }
            }
            else
            {
                book.Img = bookInDb!.Img;
            }

            _bookRepository.Update(book);
            await _bookRepository.CommitAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Book updated successfully",
                BookId = book.BookId
            });
        }

        [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var product = await _bookRepository.GetOneAsync(e => e.BookId == id, null, false, cancellationToken: cancellationToken);

            if (product is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _bookRepository.Delete(product);
            await _bookRepository.CommitAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Book deleted successfully",
                BookId = product.BookId
            });
        }
    }
}
