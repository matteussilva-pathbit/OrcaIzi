using OrcaIzi.Application;
using OrcaIzi.Infrastructure;
using OrcaIzi.WebAPI.Services;
using Microsoft.AspNetCore.Identity;
using OrcaIzi.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using OrcaIzi.WebAPI.Middleware;
using FluentValidation.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OrcaIzi.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services from other layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Caching
builder.Services.AddMemoryCache();

// Rate Limiting
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter(policyName: "fixed", opt => {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// Add Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<OrcaIzi.Infrastructure.Context.OrcaIziDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
    };
});

// Swagger Configuration for JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrcaIzi API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(); // Always use exception handler for consistent API responses
app.UseHttpsRedirection();
app.UseStaticFiles();

// Set culture to pt-BR for correct decimal parsing (49,90)
var supportedCultures = new[] { new CultureInfo("pt-BR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter(); // Add Rate Limiter Middleware

app.MapControllers();

// Seed database
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<OrcaIziDbContext>();
        var providerName = dbContext.Database.ProviderName;
        if (!string.IsNullOrWhiteSpace(providerName) && providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            try
            {
                await using (var existsCommand = connection.CreateCommand())
                {
                    existsCommand.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'";
                    var exists = await existsCommand.ExecuteScalarAsync();

                    if (exists == null)
                    {
                        await using var createHistory = connection.CreateCommand();
                        createHistory.CommandText = "CREATE TABLE [__EFMigrationsHistory] ([MigrationId] nvarchar(150) NOT NULL, [ProductVersion] nvarchar(32) NOT NULL, CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId]));";
                        await createHistory.ExecuteNonQueryAsync();
                    }
                }

                int historyCount;
                await using (var countCommand = connection.CreateCommand())
                {
                    countCommand.CommandText = "SELECT COUNT(1) FROM [__EFMigrationsHistory]";
                    historyCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                if (historyCount == 0)
                {
                    await using var customersExists = connection.CreateCommand();
                    customersExists.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customers'";
                    var hasCustomersTable = await customersExists.ExecuteScalarAsync();

                    if (hasCustomersTable != null)
                    {
                        var migrations = dbContext.Database.GetMigrations().ToList();
                        var lastMigration = migrations.LastOrDefault();
                        var version = "10.0.3";

                        foreach (var migrationId in migrations)
                        {
                            if (migrationId == lastMigration)
                            {
                                continue;
                            }

                            await using var insertCommand = connection.CreateCommand();
                            insertCommand.CommandText = "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (@id, @ver)";
                            var p1 = insertCommand.CreateParameter();
                            p1.ParameterName = "@id";
                            p1.Value = migrationId;
                            insertCommand.Parameters.Add(p1);
                            var p2 = insertCommand.CreateParameter();
                            p2.ParameterName = "@ver";
                            p2.Value = version;
                            insertCommand.Parameters.Add(p2);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Log.Warning("Pending migrations detected: {Migrations}", pendingMigrations);

                var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
                if (applyMigrations)
                {
                    await dbContext.Database.MigrateAsync();
                    Log.Information("Applied pending migrations on startup.");
                }
            }
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedService.SeedRolesAsync(roleManager);

        var userManager = services.GetRequiredService<UserManager<User>>();
        await SeedService.SeedAdminUserAsync(userManager);
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while seeding the database.");
}

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
