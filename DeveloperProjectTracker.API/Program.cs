using DeveloperProjectTracker.API.Configuration;
using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.Data.Seed;
using DeveloperProjectTracker.API.Services.Auth;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseNpgsql(
   builder.Configuration.GetConnectionString("DefaultConnection")
    ));

//JWT
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPasswordHasher<User>,
    PasswordHasher<User>>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) 
{ 
    var services = scope.ServiceProvider; 
    var context = services.GetRequiredService<ApplicationDbContext>(); 
    var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
    var configuration = services.GetRequiredService<IConfiguration>(); 
    await SuperAdminSeeder.SeedAsync(context, passwordHasher, configuration); 
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();