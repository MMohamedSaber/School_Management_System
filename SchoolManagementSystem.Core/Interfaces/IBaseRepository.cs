using System.Linq.Expressions;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] Include);
        Task<T> GetByIdAsync(int Id, params Expression<Func<T, object>>[] Include);
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
