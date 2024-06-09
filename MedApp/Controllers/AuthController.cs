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
        _authService.RegisterUser(request);
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var (accessToken, refreshToken) = _authService.LoginUser(request);
        return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken }); 
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromHeader(Name = "Authorization")] string authorizationHeader)
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