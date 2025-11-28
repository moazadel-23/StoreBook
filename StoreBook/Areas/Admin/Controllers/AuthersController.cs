using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoreBook.Models;
using StoreBook.Repositories.IRepository;

namespace StoreBook.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthersController : ControllerBase
    {
        public IRepository<Auther> _autherrepository { get; }
        public ApplicationDbContext _DbContext { get; }

        public AuthersController(IRepository<Auther> AutherRepository, ApplicationDbContext applicationDbContext)
        {
            _autherrepository = AutherRepository;
            _DbContext = applicationDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var categories = await _autherrepository.GetAsync();

            return Ok(categories.AsEnumerable().ToList());
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(Auther auther, CancellationToken cancellationToken)
        {
            await _autherrepository.AddAsync(auther, cancellationToken);
            await _autherrepository.CommitAsync(cancellationToken);

            return Ok();
        }

        [HttpPut("")]
        public async Task<IActionResult> Edit(Auther auther, CancellationToken cancellationToken)
        {
            _autherrepository.Update(auther);
            await _autherrepository.CommitAsync(cancellationToken);

            return Ok();
        }
        [HttpDelete("id")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _autherrepository.GetOneAsync(e => e.AutherId == id, cancellationToken: cancellationToken);

            _autherrepository.Delete(category!);
            await _autherrepository.CommitAsync(cancellationToken);

            return Ok();
        }
    }
}
