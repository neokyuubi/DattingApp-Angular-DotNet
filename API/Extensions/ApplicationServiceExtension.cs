using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			
			// Configure Cloudinary settings - use environment variables in production, config in development
			services.Configure<CloudinarySettings>(options =>
			{
				options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") 
					?? config["CouldinarySettings:CloudName"];
				options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") 
					?? config["CouldinarySettings:ApiKey"];
				options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") 
					?? config["CouldinarySettings:ApiSecret"];
			});
			
			services.AddScoped<IPhotoService, PhotoService>();
			services.AddScoped<LogUserActivity>();
			services.AddSignalR();
			services.AddSingleton<PresenceTracker>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}