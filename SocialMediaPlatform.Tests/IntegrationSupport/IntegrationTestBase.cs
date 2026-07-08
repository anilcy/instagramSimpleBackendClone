using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SocialMediaPlatform.Business.Mappings;
using SocialMediaPlatform.Data;
using SocialMediaPlatform.Entities.Models;

namespace SocialMediaPlatform.Tests.IntegrationSupport;


// we create a brand-new database with a
// unique name for EVERY test, and drop it afterwards. xUnit constructs a fresh
// instance of the test class per [Fact] and, because we implement IAsyncLifetime,
// awaits InitializeAsync before the test and DisposeAsync after it. Unique names
// also make parallel test classes safe 

public abstract class IntegrationTestBase : IAsyncLifetime
{
    // Local server credentials. Gotta override via env var if setup differs
 
    private static readonly string Host = Environment.GetEnvironmentVariable("TEST_PG_HOST") ?? "localhost";
    private static readonly string Port = Environment.GetEnvironmentVariable("TEST_PG_PORT") ?? "5432";
    private static readonly string User = Environment.GetEnvironmentVariable("TEST_PG_USER") ?? "postgres";
    private static readonly string Password = Environment.GetEnvironmentVariable("TEST_PG_PASSWORD") ?? "postgre123";

    // Unique database per test: smp_test_<guid>. Created in InitializeAsync, dropped in DisposeAsync.
    private readonly string _dbName = $"smp_test_{Guid.NewGuid():N}";

    private DbContextOptions<SocialMediaDbContext> _options = null!;

    // Real AutoMapper, configured exactly like Program.cs.
    protected IMapper Mapper { get; }

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(MappingProfile));
        Mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    // "Pooling=false": Npgsql normally keeps physical connections alive in a pool.
    // Pooled connections to a dropped-and-recreated test DB would go stale, and they
    // also block DROP DATABASE. With dozens of throwaway databases, pooling only hurts.
    private static string ConnectionStringFor(string database) =>
        $"Host={Host};Port={Port};Database={database};Username={User};Password={Password};Pooling=false;Include Error Detail=true";

    public async Task InitializeAsync()
    {
        // 1) Create this test's private database (via the maintenance DB "postgres").
        await using (var admin = new NpgsqlConnection(ConnectionStringFor("postgres")))
        {
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{_dbName}\"", admin);
            await cmd.ExecuteNonQueryAsync();
        }

        _options = new DbContextOptionsBuilder<SocialMediaDbContext>()
            .UseNpgsql(ConnectionStringFor(_dbName))
            .Options;

        // 2) Build the schema by running the REAL migrations
        // (Program.cs does the same in Development, so tests mirror app startup.)
        //  if InitializeAsync THROWS, xUnit
        // never calls DisposeAsync, a failing migration once left 19 orphaned
        // smp_test_* databases behind. So on failure we clean up ourselves.
        try
        {
            await using var ctx = CreateContext();
            await ctx.Database.MigrateAsync();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        // Drop the test database. WITH (FORCE) kicks any lingering connection so the
        // drop can't fail with "database is being accessed by other users".
        await using var admin = new NpgsqlConnection(ConnectionStringFor("postgres"));
        await admin.OpenAsync();
        await using var cmd = new NpgsqlCommand($"DROP DATABASE \"{_dbName}\" WITH (FORCE)", admin);
        await cmd.ExecuteNonQueryAsync();
    }

    // A FRESH context per call , same database, clean change-tracker. need to use separate
    // contexts for Arrange / Act / Assert so EF's identity cache can't hand back the
    // in-memory object and hide a "nothing was actually persisted" bug.
    protected SocialMediaDbContext CreateContext() => new SocialMediaDbContext(_options);

    // Insert a real user row so posts/likes have a valid, active author
    // (foreign keys + the "Author.IsActive" query filters require this).
    protected async Task<AppUser> SeedUserAsync(string label)
    {
        var user = new AppUser($"user_{label}", $"{label}@example.com", $"User {label}")
        {
            Id = Guid.NewGuid()
        };
        await using var ctx = CreateContext();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();
        return user;
    }
}
