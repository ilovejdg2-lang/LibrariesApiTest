using HackerRank1.DTO;
using HackerRank1.Entities;
using HackerRank1.Helpers;
using HackerRank1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank1.Controllers;

public record TokenResponse(string token);
public record UserCredential(string Username, string Password);

[ApiController]
public class AuthController : Controller
{
    private readonly IAuthenticationService authenticationService;
    
    private readonly JwtSettings jwtSettings;

    public AuthController(IAuthenticationService _authenticationService, JwtSettings _jwtSettings)
    {
        authenticationService = _authenticationService;
        jwtSettings = _jwtSettings;
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    public async Task<IActionResult> Login(UserCredential user) 
    {
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            return BadRequest("Usuario y contrasena son requeridos.");

        var validuser = await authenticationService.AuthenticateAsync(user.Username, user.Password);
        if (validuser is null)
            return Unauthorized();


        var token = TokenGenerator.GenerateToken(validuser, jwtSettings);

        return Ok(new TokenResponse(token));
    }

}
