using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var connString = "";
// if (builder.Environment.IsDevelopment()) 
//     connString = builder.Configuration.GetConnectionString("DefaultConnection");
// else 
// {
// // Use connection string provided at runtime by Heroku (PAID now so cannot test it anymore).
//         var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

//         // Parse connection URL to connection string for Npgsql
//         connUrl = connUrl.Replace("postgres://", string.Empty);
//         var pgUserPass = connUrl.Split("@")[0];
//         var pgHostPortDb = connUrl.Split("@")[1];
//         var pgHostPort = pgHostPortDb.Split("/")[0];
//         var pgDb = pgHostPortDb.Split("/")[1];
//         var pgUser = pgUserPass.Split(":")[0];
//         var pgPass = pgUserPass.Split(":")[1];
//         var pgHost = pgHostPort.Split(":")[0];
//         var pgPort = pgHostPort.Split(":")[1];

//         connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
// }
connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200")); // if no ssl certifacate is installed
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://localhost:4200"));


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
