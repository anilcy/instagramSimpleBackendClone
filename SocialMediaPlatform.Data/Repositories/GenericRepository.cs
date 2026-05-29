using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SocialMediaPlatform.Data.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly SocialMediaDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(SocialMediaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);

    public IQueryable<T> Query()
        => _dbSet.AsQueryable();

    public void Add(T entity)
        => _dbSet.Add(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}