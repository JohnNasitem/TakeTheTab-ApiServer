//***********************************************************************************
//Program: Program.cs
//Description: Main program file
//Date: Sep 16, 2025
//Author: John Nasitem
//***********************************************************************************

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using takethetab_server.Application;
using takethetab_server.Application.Interfaces;
using takethetab_server.Domain.Entities;
using takethetab_server.Infrastructure.Database;
using takethetab_server.Infrastructure.Password;
using takethetab_server.Infrastructure.Repositories;
using takethetab_server.Infrastructure.Token;
using takethetab_server.Web.Services;

namespace takethetab_server.Web
{
    public class Program
    {
        public record AccountInfo(string Username, string Password);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            InitializeServiceCollection(builder);
            builder.Services.AddControllers(options =>
            {
                // Make all endpoints require a valid jwt by default
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
            });

            // Use [AllowAnonymous] to make endpoints not require jwt token (like log in)

            TokenConfig tokenConfig = builder.Configuration.GetSection("Tokens").Get<TokenConfig>()!;

            // Set up JWT Bearer
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenConfig.JwtIssuer,
                    ValidAudience = tokenConfig.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(tokenConfig.JwtEnvironmentVariableName)!))
                };

                // Tells jwtbreaer to look for the jwt access token in the cookies
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("accessToken"))
                        {
                            context.Token = context.Request.Cookies["accessToken"];
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebsite", policy =>
                {
                    policy.WithOrigins(
                            "https://takethetab.com",
                            "https://www.takethetab.com"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });

            var app = builder.Build();

            app.UseCors("AllowWebsite");
            app.UseRouting();

            // Set up JWT Bearer
            app.UseAuthentication();
            app.UseAuthorization();

            // Enable routing + controller endpoints
            app.MapControllers();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }



        /// <summary>
        /// Sets up service collection 
        /// </summary>
        /// <param name="serviceInstance">Collection to set up</param>
        private static void InitializeServiceCollection(WebApplicationBuilder builder)
        {
            // Token config
            IServiceCollection serviceInstance = builder.Services.Configure<TokenConfig>(builder.Configuration.GetSection("Tokens"));

            // Serivces
            serviceInstance.AddSingleton<DatabaseInitializer>();
            serviceInstance.AddSingleton<UserService>();
            serviceInstance.AddSingleton<EventService>();
            serviceInstance.AddSingleton<CookieService>();
            serviceInstance.AddSingleton<AuthService>();
            serviceInstance.AddSingleton<ITokenService, TokenService>();
            serviceInstance.AddSingleton<IPasswordService, PasswordService>();

            // Repositories
            serviceInstance.AddSingleton<IUserRepository, UserRepository>();
            serviceInstance.AddSingleton<IEventRepository, EventRepository>();
            serviceInstance.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
        }
    }
}
