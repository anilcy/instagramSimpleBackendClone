using System.Linq.Expressions;

namespace instagramClone.Business.Interfaces;

public interface IGenericService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(object id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task InsertAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync();
    Task<int> SaveChangesAsync();
}