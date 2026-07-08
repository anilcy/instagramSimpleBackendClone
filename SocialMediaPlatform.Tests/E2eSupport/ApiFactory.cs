using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SocialMediaPlatform.Business.Interfaces;
using SocialMediaPlatform.Data;

namespace SocialMediaPlatform.Tests.E2eSupport;


// WebApplicationFactory<Program> runs the actual Program.cs -> the real DI
// container, the real middleware pipeline (exception handler, CORS, JWT auth),
// the real controllers and routing. Tests talk to it through an HttpClient,
// exactly like the mobile/web frontend would — no service is constructed by
// hand anymore. 

// Only two things are overridden, both justified:
//   - The database is pointed at a private local-Postgres DB (smp_e2e_<guid>),
//     created+migrated before the host starts, dropped afterwards.
//  - IFileStorageService: replaced with a fake — writing real files to disk is
//     an external side effect, not part of the HTTP contract under test.

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly string Host = Environment.GetEnvironmentVariable("TEST_PG_HOST") ?? "localhost";
    private static readonly string Port = Environment.GetEnvironmentVariable("TEST_PG_PORT") ?? "5432";
    private static readonly string User = Environment.GetEnvironmentVariable("TEST_PG_USER") ?? "postgres";
    private static readonly string Password = Environment.GetEnvironmentVariable("TEST_PG_PASSWORD") ?? "postgre123";

    private readonly string _dbName = $"smp_e2e_{Guid.NewGuid():N}";

    private static string ConnectionStringFor(string database) =>
        $"Host={Host};Port={Port};Database={database};Username={User};Password={Password};Pooling=false;Include Error Detail=true";

    //  the test host's working directory is the test
    // project's bin folder, so Program.cs's Env.Load() finds no .env there and the
    // app dies at startup with "DB_CONNECTION_STRING not found" — a failure mode no
    // lower test layer could ever see. The factory therefore provides the required
    // environment itself, before the host is built:
    //   • DB_CONNECTION_STRING just has to EXIST (the real DB comes from the DI
    //     override below; the raw startup check is skipped in Testing).
    //   • JWT_* feed BOTH sides of the token chain — TokenService (signing) and
    //     Program.cs validation — from one place, keeping them symmetric.
    static ApiFactory()
    {
        Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", ConnectionStringFor("postgres"));
        Environment.SetEnvironmentVariable("JWT_KEY", "e2e-test-signing-key-that-is-definitely-long-enough-1234");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "e2e-test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "e2e-test-audience");
        Environment.SetEnvironmentVariable("JWT_EXPIRE_MINUTES", "60");
        Environment.SetEnvironmentVariable("REDIS_CONNECTION", "localhost:6379,abortConnect=false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" environment: Program.cs skips its raw startup connection check,
        // skips Swagger/Scalar, and skips its own Migrate() (we migrate below).
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Swap the app's DbContext registration (which points at the .env
            // connection string) for our private test database. This is the
            // documented override pattern: remove the options descriptor, re-add.
            var dbOptions = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SocialMediaDbContext>));
            if (dbOptions != null)
                services.Remove(dbOptions);
            services.AddDbContext<SocialMediaDbContext>(o =>
                o.UseNpgsql(ConnectionStringFor(_dbName), b => b.MigrationsAssembly("SocialMediaPlatform.Data")));

            // Swap the real disk-writing file storage for a fake.
            var fileStorage = services.SingleOrDefault(d => d.ServiceType == typeof(IFileStorageService));
            if (fileStorage != null)
                services.Remove(fileStorage);
            services.AddScoped<IFileStorageService, FakeFileStorageService>();
        });
    }

    // Create + migrate this run's private database BEFORE the host boots.
    public async Task InitializeAsync()
    {
        await using (var admin = new NpgsqlConnection(ConnectionStringFor("postgres")))
        {
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{_dbName}\"", admin);
            await cmd.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<SocialMediaDbContext>()
            .UseNpgsql(ConnectionStringFor(_dbName))
            .Options;
        await using var ctx = new SocialMediaDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    // Tear down: dispose the host first (releases connections), then drop the DB.
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await using var admin = new NpgsqlConnection(ConnectionStringFor("postgres"));
        await admin.OpenAsync();
        await using var cmd = new NpgsqlCommand($"DROP DATABASE \"{_dbName}\" WITH (FORCE)", admin);
        await cmd.ExecuteNonQueryAsync();
    }

    // The app pipeline starts with UseHttpsRedirection; an http:// base address
    // would turn every request into a 307. Talk https from the start instead.
    public HttpClient CreateHttpsClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<string> UploadFileAsync(IFormFile file) =>
            Task.FromResult($"/uploads/fake-{Guid.NewGuid():N}.jpg");

        public Task<bool> DeleteFileAsync(string fileUrl) => Task.FromResult(true);
    }
}
