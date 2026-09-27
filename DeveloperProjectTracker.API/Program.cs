using DeveloperProjectTracker.API.Authorization;
using DeveloperProjectTracker.API.Configuration;
using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.Data.Seed;
using DeveloperProjectTracker.API.Services.Auth;
using DeveloperProjectTracker.API.Services.Projects;
using DeveloperProjectTracker.API.Services.Tenant;
using DeveloperProjectTracker.API.Services.Tenants;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// JWT settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPasswordHasher<User>,
    PasswordHasher<User>>();

// Tenant
builder.Services.AddScoped<ITenantService, TenantService>();

builder.Services.AddScoped<
    ITenantMembershipService,
    TenantMembershipService>();

builder.Services.AddScoped<
    ITenantAuthorizationService,
    TenantAuthorizationService>();

builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantAdmin", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new TenantAdminRequirement());
    });
});


builder.Services.AddScoped<
    IAuthorizationHandler,
    TenantAdminHandler>();

//Project
builder.Services.AddScoped<IProjectService, ProjectService>();

// =====================================================
// JWT AUTHENTICATION
// =====================================================

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

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

            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();


// =====================================================
// SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DeveloperProjectTracker.API",
        Version = "v1"
    });

    // Bearer JWT
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description =
            "Enter your JWT token. Example: Bearer eyJhbGciOi..."
    });

    // .NET 10 / Swashbuckle 10 syntax
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "bearer",
                document)] = []
        });
});


var app = builder.Build();


// =====================================================
// SUPER ADMIN SEEDER
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<ApplicationDbContext>();

    var passwordHasher =
        services.GetRequiredService<IPasswordHasher<User>>();

    var configuration =
        services.GetRequiredService<IConfiguration>();

    await SuperAdminSeeder.SeedAsync(
        context,
        passwordHasher,
        configuration);
}


// =====================================================
// HTTP PIPELINE
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();