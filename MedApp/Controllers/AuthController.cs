using MedApp.Contracts;
using MedApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var existingUser = _authService.GetUserByEmail(request.Email);
        if (existingUser!= null)
        {
            return Conflict("User already exists. Please log in.");
        }

        try
        {
            _authService.RegisterUser(request);
            return Ok(new { message = "User registered successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest("Failed to register user. Please try again.");
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var (accessToken, refreshToken) = _authService.LoginUser(request);
        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromHeader(Name = "authorizationHeader")] string authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return BadRequest("Invalid authorization header");
        }

        var refreshToken = authorizationHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var (accessToken, newRefreshToken) = _authService.RefreshToken(refreshToken);
            return Ok(new { AccessToken = accessToken, RefreshToken = newRefreshToken });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }

}