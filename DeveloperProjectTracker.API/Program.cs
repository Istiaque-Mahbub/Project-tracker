using DeveloperProjectTracker.API.Authorization;
using DeveloperProjectTracker.API.Configuration;
using DeveloperProjectTracker.API.Data;
using DeveloperProjectTracker.API.Data.Seed;
using DeveloperProjectTracker.API.Hubs;
using DeveloperProjectTracker.API.Services.ActivityLogs;
using DeveloperProjectTracker.API.Services.Auth;
using DeveloperProjectTracker.API.Services.Comments;
using DeveloperProjectTracker.API.Services.Labels;
using DeveloperProjectTracker.API.Services.Milestones;
using DeveloperProjectTracker.API.Services.Notifications;
using DeveloperProjectTracker.API.Services.ProjectMembers;
using DeveloperProjectTracker.API.Services.Projects;
using DeveloperProjectTracker.API.Services.Task;
using DeveloperProjectTracker.API.Services.Tenant;
using DeveloperProjectTracker.API.Services.Tenants;
using DeveloperProjectTracker.API.Services.WorkLogs;
using DeveloperProjectTrackerDomain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SIGNALR
// =====================================================

builder.Services.AddSignalR();

// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddHttpContextAccessor();

// =====================================================
// DATABASE
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")
    ));

// =====================================================
// JWT SETTINGS
// =====================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// =====================================================
// AUTH SERVICES
// =====================================================

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPasswordHasher<User>,
    PasswordHasher<User>>();

// =====================================================
// TENANT SERVICES
// =====================================================

builder.Services.AddScoped<
    ITenantService,
    TenantService>();

builder.Services.AddScoped<
    ITenantMembershipService,
    TenantMembershipService>();

builder.Services.AddScoped<
    ITenantAuthorizationService,
    TenantAuthorizationService>();

// =====================================================
// TENANT AUTHORIZATION
// =====================================================

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

// =====================================================
// PROJECT
// =====================================================

builder.Services.AddScoped<
    IProjectService,
    ProjectService>();

// =====================================================
// TASK
// =====================================================

builder.Services.AddScoped<
    ITaskService,
    TaskService>();

// =====================================================
// PROJECT MEMBERS
// =====================================================

builder.Services.AddScoped<
    IProjectMemberService,
    ProjectMemberService>();

// =====================================================
// MILESTONE
// =====================================================

builder.Services.AddScoped<
    IMilestoneService,
    MilestoneService>();

// =====================================================
// COMMENTS
// =====================================================

builder.Services.AddScoped<
    ICommentService,
    CommentService>();

// =====================================================
// LABELS
// =====================================================

builder.Services.AddScoped<
    ILabelService,
    LabelService>();

// =====================================================
// WORK LOGS
// =====================================================

builder.Services.AddScoped<
    IWorkLogService,
    WorkLogService>();

// =====================================================
// NOTIFICATIONS
// =====================================================

builder.Services.AddScoped<
    INotificationService,
    NotificationService>();


// =====================================================
// NOTIFICATIONS
// =====================================================

builder.Services.AddScoped<
    IActivityLogService,
    ActivityLogService>();

// =====================================================
// JWT AUTHENTICATION
// =====================================================

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwtSettings is null)
{
    throw new InvalidOperationException(
        "JWT settings are not configured.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,

                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key))
            };

        // =================================================
        // SignalR JWT Authentication
        // =================================================

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =
                    context.Request.Query["access_token"];

                var path =
                    context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments(
                        "/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// =====================================================
// SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "DeveloperProjectTracker.API",
            Version = "v1"
        });

    // =================================================
    // JWT Bearer Authentication
    // =================================================

    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            Description =
                "Enter your JWT token. Example: " +
                "Bearer eyJhbGciOi..."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "bearer",
                    document)
            ] = []
        });
});

// =====================================================
// BUILD APPLICATION
// =====================================================

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
        services.GetRequiredService<
            IPasswordHasher<User>>();

    var configuration =
        services.GetRequiredService<IConfiguration>();

    await SuperAdminSeeder.SeedAsync(
        context,
        passwordHasher,
        configuration);
}

// =====================================================
// DEVELOPMENT
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// =====================================================
// HTTP PIPELINE
// =====================================================

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();

// =====================================================
// SIGNALR HUB
// =====================================================

app.MapHub<NotificationHub>(
    "/hubs/notifications");

// =====================================================
// RUN
// =====================================================

app.Run();