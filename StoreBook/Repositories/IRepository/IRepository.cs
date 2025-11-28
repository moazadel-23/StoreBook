using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace StoreBook.Repositories.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);

        void Delete(T entity);

        Task CommitAsync(CancellationToken cancellationToken);


        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = default,
            Expression<Func<T, object>>[]? include = null,
            bool tracked = true,
            CancellationToken CancellationToken = default);

         Task<T?> GetOneAsync(
         Expression<Func<T, bool>> expression,
         Expression<Func<T, object>>[]? include = null,
         bool traced = true,
         CancellationToken cancellationToken = default);
     
    }
}
