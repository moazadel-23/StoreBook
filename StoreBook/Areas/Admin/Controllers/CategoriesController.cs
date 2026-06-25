using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoreBook.Models;
using StoreBook.Repositories.IRepository;

namespace StoreBook.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        public IRepository<Category> _categoryRepository { get; }
        public ApplicationDbContext _DbContext { get; }

        public CategoriesController(IRepository<Category> CategoryRepository, ApplicationDbContext applicationDbContext)
        {
            _categoryRepository = CategoryRepository;
            _DbContext = applicationDbContext;
        }
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAsync();

            return Ok(categories.AsEnumerable().ToList());
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
        {

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _categoryRepository.CommitAsync(cancellationToken);

            //return View(nameof(Index));
            return Ok();
        }

        [HttpPut("")]
        public async Task<IActionResult> Edit(Category category, CancellationToken cancellationToken)
        {
            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync(cancellationToken);

            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (category is null)
            {
                return NotFound(new { message = "Category not found" });
            }

            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync(cancellationToken);

            return Ok();
        }
    }
}
