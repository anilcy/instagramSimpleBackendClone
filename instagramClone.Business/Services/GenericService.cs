using System.Linq.Expressions;
using instagramClone.Business.Interfaces;
using instagramClone.Data.Interfaces;

namespace instagramClone.Business.Services;

public class GenericService<T> : IGenericService<T> where T : class
{
    private readonly IGenericRepository<T> _repository;

    public GenericService(IGenericRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _repository.FindAsync(predicate);
    }

    public async Task InsertAsync(T entity)
    {
        await _repository.InsertAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        await _repository.DeleteAsync(entity);
    }

    public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        await _repository.DeleteAsync(predicate);
    }

    public async Task<int> CountAsync()
    {
        return await _repository.CountAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _repository.SaveChangesAsync();
    }
}
