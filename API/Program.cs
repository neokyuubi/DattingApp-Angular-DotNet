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
if (builder.Environment.IsDevelopment()) 
{
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
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
            var pgDb = hostDbParts[1].Split("?")[0]; // Remove query parameters if any
            
            var userPassParts = pgUserPass.Split(":");
            if (userPassParts.Length < 2) throw new ArgumentException("Invalid DATABASE_URL format");
            
            var pgUser = Uri.UnescapeDataString(userPassParts[0]);
            var pgPass = Uri.UnescapeDataString(string.Join(":", userPassParts.Skip(1))); // Handle passwords with colons
            
            var hostPortParts = pgHostPort.Split(":");
            var pgHost = hostPortParts[0];
            var pgPort = hostPortParts.Length > 1 ? hostPortParts[1] : "5432";

            connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true;";
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
    // In production, allow requests from the Render frontend URL
    app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://dating-app-jiu1.onrender.com"));
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
