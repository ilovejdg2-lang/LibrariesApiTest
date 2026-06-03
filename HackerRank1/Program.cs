using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LibraryService.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LoadEnvFile();
            EnsureLocalDevelopmentDefault();

            var host = CreateHostBuilder(args).Build();
            host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
            {
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT")))
                    Console.WriteLine("Swagger UI: http://localhost:5219/swagger");
            });
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    var port = Environment.GetEnvironmentVariable("PORT");
                    if (!string.IsNullOrWhiteSpace(port))
                    {
                        webBuilder.UseUrls($"http://*:{port}");
                    }
                    else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
                    {
                        webBuilder.UseUrls("http://localhost:5219");
                    }
                });

        private static void LoadEnvFile()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                var envPath = Path.Combine(directory.FullName, ".env");
                if (File.Exists(envPath))
                {
                    LoadEnvFromPath(envPath);
                    return;
                }

                directory = directory.Parent;
            }
        }

        private static void LoadEnvFromPath(string envPath)
        {
            foreach (var rawLine in File.ReadAllLines(envPath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith('#'))
                    continue;

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim().Trim('"');
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        private static void EnsureLocalDevelopmentDefault()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT")))
                return;

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_IIS_HTTPAUTH")))
                return;

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        }
    }
}
