using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SocialMediaPlatform.Data.Interfaces;

public interface IGenericRepository<T> where T : class
{
    // Queries
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    IQueryable<T> Query(); // checks sometinhg in the database instead of loading everything into memory and then checking

    // Mutations
    void Add(T entity);
    void Remove(T entity);  // hard delete in case we need, not async cause it takes a relly short time

    // Persistence
    Task<int> SaveChangesAsync();
}
