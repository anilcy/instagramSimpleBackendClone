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
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<SignInManager<AppUser>>();

builder.Services.AddIdentity<AppUser, AppRole>()
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
    });

var app = builder.Build();

app.UseHttpsRedirection();

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

// JWT Authentication
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.Run();
