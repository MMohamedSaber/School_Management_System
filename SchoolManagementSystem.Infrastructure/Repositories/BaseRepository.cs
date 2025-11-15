using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {


        public Task AddAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<T>> GetAllAsync(params System.Linq.Expressions.Expression<Func<T, object>>[] Include)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByIdAsync(int Id, params System.Linq.Expressions.Expression<Func<T, object>>[] Include)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
