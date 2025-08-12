using System.IdentityModel.Tokens.Jwt;
using DotNetEnv;
using instagramClone.Business.Interfaces;
using instagramClone.Business.Services;
using instagramClone.Data;
using instagramClone.Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using instagramClone.Business.Mappings;
using instagramClone.Data.Interfaces;
using instagramClone.Data.Repositories;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "InstagramClone API", 
        Version = "v1" 
    });

    // JWT Bearer’ı Http tipiyle tanımlıyoruz:
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token. Sadece token kısmını girin.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,    // ← Burayı ApiKey yerine Http yapıyoruz
        Scheme = "bearer",                 // ← küçük harf “bearer”
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            // Burada da tanımladığımız Bearer’ı kullanıyoruz:
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<SignInManager<AppUser>>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPrivacyService, PrivacyService>();

builder.Services.AddIdentityCore<AppUser>(options => { })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<InstagramDbContext>()
    .AddDefaultTokenProviders();
 
// Load .env
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// HİBRİT GETTER: önce .env (Env.GetString), yoksa gerçek environment
string GetEnv(string k) =>
    Env.GetString(k) ?? Environment.GetEnvironmentVariable(k)
    ?? throw new Exception($"⚠️ {k} not found in .env or environment variables");

var connectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DB_CONNECTION_STRING not found.");


// DbContext added to services
builder.Services.AddDbContext<InstagramDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("instagramClone.Data")));

// JWT Yapılandırması
var jwtKey      = GetEnv("JWT_KEY");
var jwtIssuer   = GetEnv("JWT_ISSUER");
var jwtAudience = GetEnv("JWT_AUDIENCE");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = key
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("OnChallenge triggered: " + context.ErrorDescription);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    if (authHeader.StartsWith("Bearer "))
                        context.Token = authHeader.Substring(7);
                    else
                        context.Token = authHeader;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations only in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InstagramDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();

// JWT Authentication
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// PostgreSQL connection test
using (var conn = new NpgsqlConnection(connectionString))
{
    conn.Open();
    using (var cmd = new NpgsqlCommand("SELECT version()", conn))
    {
        var version = cmd.ExecuteScalar()?.ToString();
        Console.WriteLine($"✅ PostgreSQL version: {version}");
    }
}


// Kök isteği / → /scalar/v1 yönlendir
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1");
    return Task.CompletedTask;
});

if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON’ınızı /openapi/v1.json altına taşıyın
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    
    // Scalar UI
    app.MapScalarApiReference(o =>
            o.WithTheme(ScalarTheme.DeepSpace)   // İstediğiniz temayı seçin
    );
}

app.Run();
