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

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Swagger configuration with JWT Security Scheme eklendi
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "InstagramClone API", 
        Version = "v1" 
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' [space] and then your valid token. Example: 'Bearer eyJhbGciOi...'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

// services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<SignInManager<AppUser>>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>(); 
builder.Services.AddScoped<IFileStorageService, FileStorageService>(); 
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        // Identity seçeneklerinizi buraya ekleyebilirsiniz
    })
    .AddRoles<AppRole>()  // Roller kullanacaksanız ekleyin, kullanmayacaksanız bu satırı kaldırabilirsiniz
    .AddEntityFrameworkStores<InstagramDbContext>()
    .AddDefaultTokenProviders();


// load .env
Env.Load();
builder.Configuration.AddEnvironmentVariables();
var connectionString = Env.GetString("DB_CONNECTION_STRING");
Console.WriteLine($"🔹 JWT_KEY: {Env.GetString("JWT_KEY")}");
Console.WriteLine($"🔹 JWT_ISSUER: {Env.GetString("JWT_ISSUER")}");
Console.WriteLine($"🔹 JWT_AUDIENCE: {Env.GetString("JWT_AUDIENCE")}");

// DbContext added to services
builder.Services.AddDbContext<InstagramDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("instagramClone.Data")));

var jwtKey = Env.GetString("JWT_KEY");
var jwtIssuer = Env.GetString("JWT_ISSUER");
var jwtAudience = Env.GetString("JWT_AUDIENCE");

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new Exception("⚠️ JWT yapılandırması eksik! Lütfen .env dosyanı kontrol et.");
}

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

var app = builder.Build();

app.UseHttpsRedirection();


// JWT Authentication
app.UseRouting();
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

// DbContext
app.Services.CreateScope().ServiceProvider.GetRequiredService<InstagramDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
