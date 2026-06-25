using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StoreBook.Repositories.IRepository;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
namespace StoreBook.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private  ApplicationDbContext _dbContext { get; }
        private readonly DbSet<T> dbSet;
        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            dbSet = _dbContext.Set<T>();
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {

            var result = await dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }
        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = default,
            Expression<Func<T, object>>[]? include = null,
            bool tracked = true,
            CancellationToken CancellationToken = default)
        {
            IQueryable<T> query = dbSet.AsQueryable();
            if(expression is not null)
            {
                query = query.Where(expression);
            }
            if (include is not null)
            {
                foreach (var includee in include)
                {
                    query = query.Include(includee);
                }
            }
            if(!tracked)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync(cancellationToken: CancellationToken);
        }
        public async Task<T?> GetOneAsync(
         Expression<Func<T, bool>> expression,
         Expression<Func<T, object>>[]? include = null,
         bool tracked = true,
         CancellationToken cancellationToken = default)
        {
            var result = await GetAsync(expression, include, tracked, cancellationToken);
            return result.FirstOrDefault();
        }
    }
}
