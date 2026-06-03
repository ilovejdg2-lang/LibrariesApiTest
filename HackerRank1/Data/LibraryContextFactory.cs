using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LibraryService.WebAPI.Data;

public class LibraryContextFactory : IDesignTimeDbContextFactory<LibraryContext>
{
    public LibraryContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required in appsettings.Development.json");

        var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LibraryContext(optionsBuilder.Options);
    }
}
