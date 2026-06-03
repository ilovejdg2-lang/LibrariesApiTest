using HackerRank1.Entities;
using HackerRank1.Services;
using LibraryService.WebAPI.Data;
using LibraryService.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace LibraryService.WebAPI
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var corsOrigins = Configuration["Cors:Origins"]?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? new[] { "http://localhost:5173" };

            services.AddCors(options =>
            {
                options.AddPolicy("AppCors", policy =>
                    policy.WithOrigins(corsOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

            var isDevelopment = _env.IsDevelopment();
            var connectionString = Configuration.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("SUPABASE_DB_URL");
            var usePostgres = !string.IsNullOrWhiteSpace(connectionString);

            if (usePostgres && isDevelopment)
            {
                connectionString = PostgresConnectionHelper.ResolveAndNormalize(Configuration, testConnection: true);
                usePostgres = !string.IsNullOrWhiteSpace(connectionString);
                if (!usePostgres)
                {
                    Console.WriteLine("No database connection. Using in-memory store.");
                }
            }
            else if (usePostgres)
            {
                connectionString = PostgresConnectionHelper.ResolveAndNormalize(Configuration, testConnection: false);
                usePostgres = !string.IsNullOrWhiteSpace(connectionString);
            }

            if (usePostgres)
            {
                services.AddDbContext<LibraryContext>(options =>
                    options.UseNpgsql(connectionString!));
            }
            else if (isDevelopment)
            {
                services.AddDbContext<LibraryContext>(options =>
                    options.UseInMemoryDatabase("LibraryDb"));
            }
            else
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
            }

            var jwtSettings = Configuration
                                .GetSection("JwtSettings")
                                .Get<JwtSettings>()
                                ?? throw new InvalidOperationException("Invalid JWT Settings");

            if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
                throw new InvalidOperationException("JwtSettings:SecretKey is required.");

            services.Configure<AuthCredentials>(Configuration.GetSection("AuthCredentials"));
            var authCredentials = Configuration.GetSection("AuthCredentials").Get<AuthCredentials>()
                ?? throw new InvalidOperationException("AuthCredentials section is missing.");

            if (string.IsNullOrWhiteSpace(authCredentials.Username) || string.IsNullOrWhiteSpace(authCredentials.Password))
                throw new InvalidOperationException("AuthCredentials:Username and Password are required.");

            services.AddSingleton(jwtSettings);
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            services.AddScoped<ILibrariesService, LibrariesService>();
            services.AddScoped<IBooksService, BooksService>();

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LibraryService API",
                    Version = "v1",
                    Description = "A simple example ASP.NET Core Web API for LibraryService"
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
                try
                {
                    if (db.Database.IsRelational())
                        db.Database.Migrate();
                    else
                        db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database init failed: {ex.Message}");
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryService API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseCors("AppCors");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect("/swagger");
                    return Task.CompletedTask;
                });

                endpoints.MapControllers();
            });
        }

    }
}
