using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtension
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
			services.AddIdentityCore<AppUser>(options=>
			{
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;
				options.Password.RequiredLength = 4;
			})
			.AddRoles<AppRole>()
			.AddRoleManager<RoleManager<AppRole>>()
			.AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TokenKey") ?? config["TokenKey"])),
                    ValidateIssuer = false, // API is issuer
                    ValidateAudience = false
                };

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context => 
					{
						var accessToken = context.Request.Query["access_token"];
						var path = context.HttpContext.Request.Path;
						if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))	
						{
							context.Token = accessToken;
						}
						return Task.CompletedTask;
					}
				};
            });

			services.AddAuthorization(Options => 
			{
				Options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
				Options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
			});

            return services;
        }
    }
}
