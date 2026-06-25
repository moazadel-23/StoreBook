using Azure.Core;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreBook.DTOs.Request;
using StoreBook.Models;
using StoreBook.Repositories.IRepository;

namespace StoreBook.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> GetAll([FromQuery] FilterBookRequest filterBookRequest, CancellationToken cancellationToken, [FromQuery] int page = 1)
        {
            if (page < 1)
                page = 1;

            System.Linq.Expressions.Expression<Func<Book, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(filterBookRequest.Title) && filterBookRequest.CategoryId > 0)
            {
                string titleTrim = filterBookRequest.Title.Trim();
                filter = e => e.Title.Contains(titleTrim) && e.CategoryId == filterBookRequest.CategoryId;
                filterBookRequest.Title = titleTrim;
            }
            else if (!string.IsNullOrWhiteSpace(filterBookRequest.Title))
            {
                string titleTrim = filterBookRequest.Title.Trim();
                filter = e => e.Title.Contains(titleTrim);
                filterBookRequest.Title = titleTrim;
            }
            else if (filterBookRequest.CategoryId > 0)
            {
                filter = e => e.CategoryId == filterBookRequest.CategoryId;
            }

            var books = await _bookRepository.GetAsync(
                expression: filter,
                include: new System.Linq.Expressions.Expression<Func<Book, object>>[]
                {
                    e => e.Category!,
                    e => e.Brand!
                },
                tracked: false,
                CancellationToken: cancellationToken);

            // Pagination
            int totalItems = books.Count();
            const int pageSize = 10;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var pagination = new PaginationRequest
            {
                TotalPages = totalPages,
                CurrentPage = page
            };

            var booksOnPage = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                Books = booksOnPage,
                Pagination = pagination,
                filter = filterBookRequest
            });
        }

        [HttpPost("")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateBookRequest createBookRequest, CancellationToken cancellationToken)
        {
            await using var transaction = await _DbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                Book book = createBookRequest.Adapt<Book>();

                // Ensure directory exists
                string imagesRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(imagesRoot);

                if (createBookRequest.MainImg != null && createBookRequest.MainImg.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBookRequest.MainImg.FileName);
                    string filePath = Path.Combine(imagesRoot, fileName);

                    await using (var stream = System.IO.File.Create(filePath))
                    {
                        await createBookRequest.MainImg.CopyToAsync(stream, cancellationToken);
                    }

                    book.Img = fileName;
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
            catch (Exception ex)
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
        public async Task<IActionResult> Edit(int id, [FromForm] Book book, IFormFile? img, CancellationToken cancellationToken)
        {
            var bookInDb = await _bookRepository.GetOneAsync(e => e.BookId == id, tracked: false, cancellationToken: cancellationToken);

            if (bookInDb is null)
            {
                return NotFound(new { success = false, message = "Book not found" });
            }

            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await img.CopyToAsync(stream, cancellationToken);
                }

                // Remove old Img in wwwroot
                if (!string.IsNullOrEmpty(bookInDb.Img))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", bookInDb.Img);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Save Img in db
                book.Img = fileName;
            }
            else
            {
                book.Img = bookInDb.Img;
            }

            book.BookId = id;
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
                return NotFound(new { success = false, message = "Book not found" });

            // Remove old Img in wwwroot
            if (!string.IsNullOrEmpty(product.Img))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.Img);
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
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
