using Microsoft.Extensions.Configuration;

namespace LibraryService.WebAPI.Data;

public static class PostgresConnectionHelper
{
    public static string? ResolveAndNormalize(IConfiguration configuration, bool testConnection)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = Environment.GetEnvironmentVariable("SUPABASE_DB_URL");

        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        if (!testConnection)
            return DatabaseConnectionProbe.NormalizeForLocalNetwork(connectionString) ?? connectionString;

        return DatabaseConnectionProbe.ResolveAndTest(connectionString);
    }
}
