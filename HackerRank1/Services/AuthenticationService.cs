using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.Extensions.Options;

namespace HackerRank1.Services;

public interface IAuthenticationService
{
    Task<User?> AuthenticateAsync(string username, string password);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly AuthCredentials _credentials;

    public AuthenticationService(IOptions<AuthCredentials> credentials)
    {
        _credentials = credentials.Value;
    }

    public Task<User?> AuthenticateAsync(string username, string password)
    {
        if (
            string.Equals(username, _credentials.Username, StringComparison.Ordinal)
            && string.Equals(password, _credentials.Password, StringComparison.Ordinal)
        )
        {
            return Task.FromResult<User?>(
                new User
                {
                    Id = 1,
                    Username = username,
                    Password = string.Empty,
                    Role = _credentials.Role,
                }
            );
        }

        return Task.FromResult<User?>(null);
    }
}
