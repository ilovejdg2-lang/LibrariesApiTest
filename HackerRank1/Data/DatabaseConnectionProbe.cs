using System.Net;
using System.Net.Sockets;
using Npgsql;

namespace LibraryService.WebAPI.Data;

public static class DatabaseConnectionProbe
{
    public static bool CanConnect(string connectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return false;
        }
    }

    public static string? NormalizeForLocalNetwork(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        if (connectionString.Contains("pooler.supabase.com", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("[", StringComparison.Ordinal))
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var projectRef = ExtractProjectRef(builder.Host);
        if (projectRef is null)
            return connectionString;

        var ipv6Host = ResolveIpv6Host(builder.Host!);
        if (ipv6Host is not null)
        {
            builder.Host = ipv6Host;
            builder.Username = "postgres";
            return builder.ConnectionString;
        }

        builder.Host = "aws-0-us-west-2.pooler.supabase.com";
        builder.Port = 5432;
        builder.Username = $"postgres.{projectRef}";
        return builder.ConnectionString;
    }

    public static string? ResolveAndTest(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        var candidates = new List<string?>
        {
            connectionString,
            NormalizeForLocalNetwork(connectionString),
        };

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var projectRef = ExtractProjectRef(builder.Host);
        if (projectRef is not null)
        {
            candidates.Add(BuildPooler(projectRef, builder.Password, "aws-0-us-west-2.pooler.supabase.com", 6543));
            candidates.Add(BuildPooler(projectRef, builder.Password, "aws-1-us-west-2.pooler.supabase.com", 5432));
            candidates.Add(BuildPooler(projectRef, builder.Password, "aws-0-us-west-2.pooler.supabase.com", 5432));
        }

        foreach (var candidate in candidates.Distinct(StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            if (CanConnect(candidate))
            {
                Console.WriteLine("Database connected.");
                return candidate;
            }
        }

        return null;
    }

    private static string BuildPooler(string projectRef, string? password, string host, int port)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = "postgres",
            Username = $"postgres.{projectRef}",
            Password = password,
            SslMode = SslMode.Require,
            TrustServerCertificate = true,
        };

        return builder.ConnectionString;
    }

    private static string? ResolveIpv6Host(string host)
    {
        try
        {
            var addresses = Dns.GetHostAddresses(host);
            var ipv6 = addresses.FirstOrDefault(address =>
                address.AddressFamily == AddressFamily.InterNetworkV6);

            return ipv6 is null ? null : $"[{ipv6}]";
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractProjectRef(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return null;

        host = host.Trim('[', ']');

        if (!host.StartsWith("db.", StringComparison.OrdinalIgnoreCase))
            return null;

        const string suffix = ".supabase.co";
        if (!host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            return null;

        return host[3..^suffix.Length];
    }
}
