using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var connString = "";
// Check environment variable first (works for both Docker and local)
var envConnUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(envConnUrl))
{
    // Use environment variable (from .env file or Docker)
    try
    {
        var connUrl = envConnUrl.Replace("postgresql://", string.Empty).Replace("postgres://", string.Empty);
        var parts = connUrl.Split("@");
        if (parts.Length == 2)
        {
            var pgUserPass = parts[0];
            var pgHostPortDb = parts[1];
            var hostDbParts = pgHostPortDb.Split("/");
            if (hostDbParts.Length >= 2)
            {
                var pgHostPort = hostDbParts[0];
                var dbAndParams = hostDbParts[1];
                var pgDb = dbAndParams.Split("?")[0];
                
                // Check for SSL mode in query parameters
                var sslMode = "Require";
                if (dbAndParams.Contains("?"))
                {
                    var queryParams = dbAndParams.Split("?")[1];
                    if (queryParams.Contains("sslmode=disable") || queryParams.Contains("SSL%20Mode=Disable"))
                    {
                        sslMode = "Disable";
                    }
                }
                
                var userPassParts = pgUserPass.Split(":");
                if (userPassParts.Length >= 2)
                {
                    var pgUser = Uri.UnescapeDataString(userPassParts[0]);
                    var pgPass = Uri.UnescapeDataString(string.Join(":", userPassParts.Skip(1)));
                    var hostPortParts = pgHostPort.Split(":");
                    var pgHost = hostPortParts[0];
                    var pgPort = hostPortParts.Length > 1 ? hostPortParts[1] : "5432";
                    
                    // Auto-detect local databases and disable SSL
                    var isLocalDb = pgHost == "postgres" || pgHost == "localhost" || pgHost == "127.0.0.1" || pgHost.StartsWith("172.") || pgHost.StartsWith("192.168.");
                    if (isLocalDb && sslMode == "Require")
                    {
                        sslMode = "Disable";
                    }
                    
                    if (sslMode == "Disable")
                    {
                        connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Disable;";
                    }
                    else
                    {
                        connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true;";
                    }
                }
            }
        }
        if (string.IsNullOrEmpty(connString))
        {
            // Fallback to config if parsing fails
            connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
        }
    }
    catch
    {
        // Fallback to config if parsing fails
        connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    }
}
else if (builder.Environment.IsDevelopment()) 
{
    // In development, try to parse if it's a URL format, otherwise use directly
    var devConnString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (devConnString != null && (devConnString.StartsWith("postgres://") || devConnString.StartsWith("postgresql://")))
    {
        // Parse URL format connection string
        try
        {
            var connUrl = devConnString.Replace("postgresql://", string.Empty).Replace("postgres://", string.Empty);
            var parts = connUrl.Split("@");
            if (parts.Length == 2)
            {
                var pgUserPass = parts[0];
                var pgHostPortDb = parts[1];
                var hostDbParts = pgHostPortDb.Split("/");
                if (hostDbParts.Length >= 2)
                {
                    var pgHostPort = hostDbParts[0];
                    var pgDb = hostDbParts[1].Split("?")[0];
                    var userPassParts = pgUserPass.Split(":");
                    if (userPassParts.Length >= 2)
                    {
                        var pgUser = Uri.UnescapeDataString(userPassParts[0]);
                        var pgPass = Uri.UnescapeDataString(string.Join(":", userPassParts.Skip(1)));
                    var hostPortParts = pgHostPort.Split(":");
                    var pgHost = hostPortParts[0];
                    var pgPort = hostPortParts.Length > 1 ? hostPortParts[1] : "5432";
                    
                    // Check for SSL mode in query parameters
                    var sslMode = "Require";
                    if (pgDb.Contains("?"))
                    {
                        var dbAndParams = pgDb;
                        pgDb = dbAndParams.Split("?")[0];
                        var queryParams = dbAndParams.Split("?")[1];
                        if (queryParams.Contains("sslmode=disable") || queryParams.Contains("SSL%20Mode=Disable"))
                        {
                            sslMode = "Disable";
                        }
                    }
                    
                    // Auto-detect local databases and disable SSL
                    var isLocalDb = pgHost == "postgres" || pgHost == "localhost" || pgHost == "127.0.0.1" || pgHost.StartsWith("172.") || pgHost.StartsWith("192.168.");
                    if (isLocalDb && sslMode == "Require")
                    {
                        sslMode = "Disable";
                    }
                    
                    if (sslMode == "Disable")
                    {
                        connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Disable;";
                    }
                    else
                    {
                        connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true;";
                    }
                    }
                }
            }
            if (string.IsNullOrEmpty(connString))
            {
                connString = devConnString; // Fallback to original if parsing fails
            }
        }
        catch
        {
            connString = devConnString; // Fallback to original if parsing fails
        }
    }
    else
    {
        connString = devConnString ?? "";
    }
}
else 
{
    // Use connection string provided at runtime by Render (or Heroku)
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrEmpty(connUrl))
    {
        try
        {
            // Parse connection URL to connection string for Npgsql
            // Format: postgres://user:password@host:port/database or postgresql://user:password@host:port/database
            // Remove both postgres:// and postgresql:// prefixes
            connUrl = connUrl.Replace("postgresql://", string.Empty).Replace("postgres://", string.Empty);
            var parts = connUrl.Split("@");
            if (parts.Length != 2) throw new ArgumentException("Invalid DATABASE_URL format");
            
            var pgUserPass = parts[0];
            var pgHostPortDb = parts[1];
            
            var hostDbParts = pgHostPortDb.Split("/");
            if (hostDbParts.Length < 2) throw new ArgumentException("Invalid DATABASE_URL format");
            
            var pgHostPort = hostDbParts[0];
            var dbAndParams = hostDbParts[1];
            var pgDb = dbAndParams.Split("?")[0]; // Get database name
            
            // Check for SSL mode in query parameters
            var sslMode = "Require";
            if (dbAndParams.Contains("?"))
            {
                var queryParams = dbAndParams.Split("?")[1];
                if (queryParams.Contains("sslmode=disable") || queryParams.Contains("SSL%20Mode=Disable"))
                {
                    sslMode = "Disable";
                }
            }
            
            var userPassParts = pgUserPass.Split(":");
            if (userPassParts.Length < 2) throw new ArgumentException("Invalid DATABASE_URL format");
            
            var pgUser = Uri.UnescapeDataString(userPassParts[0]);
            var pgPass = Uri.UnescapeDataString(string.Join(":", userPassParts.Skip(1))); // Handle passwords with colons
            
            var hostPortParts = pgHostPort.Split(":");
            var pgHost = hostPortParts[0];
            var pgPort = hostPortParts.Length > 1 ? hostPortParts[1] : "5432";

            // Auto-detect local databases (postgres container, localhost, 127.0.0.1) and disable SSL
            var isLocalDb = pgHost == "postgres" || pgHost == "localhost" || pgHost == "127.0.0.1" || pgHost.StartsWith("172.") || pgHost.StartsWith("192.168.");
            if (isLocalDb && sslMode == "Require")
            {
                sslMode = "Disable";
            }

            if (sslMode == "Disable")
            {
                connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Disable;";
            }
            else
            {
                connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true;";
            }
        }
        catch (Exception ex)
        {
            var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
            logger?.LogError(ex, "Failed to parse DATABASE_URL, falling back to configuration");
            connString = builder.Configuration.GetConnectionString("DefaultConnection");
        }
    }
    else
    {
        connString = builder.Configuration.GetConnectionString("DefaultConnection");
    }
}
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (builder.Environment.IsDevelopment())
{
    app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://localhost:4200"));
}
else
{
    // In production, allow requests from the Render frontend URL and localhost (for local Docker testing)
    var allowedOrigins = new[] { "https://dating-app-jiu1.onrender.com", "http://localhost:4200", "https://localhost:4200", "http://localhost:3000" };
    app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(allowedOrigins));
}


app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
	var context = services.GetRequiredService<DataContext>();
	var userManager = services.GetRequiredService<UserManager<AppUser>>();
	var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
	await context.Database.MigrateAsync();
	await Seed.ClearConnections(context);
	await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
	var logger = services.GetService<ILogger<Program>>();
	logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
}

app.Run();
