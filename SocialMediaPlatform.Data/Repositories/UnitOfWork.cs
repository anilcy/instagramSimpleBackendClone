using System.Threading;
using System.Threading.Tasks;
using SocialMediaPlatform.Data.Interfaces;

namespace SocialMediaPlatform.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SocialMediaDbContext _dbContext;

    public UnitOfWork(SocialMediaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
