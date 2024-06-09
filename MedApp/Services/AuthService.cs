using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MedApp.Context;
using MedApp.Contracts;
using MedApp.Models;
using MedApp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MedApp.Services;

public interface IAuthService
{
    void RegisterUser(RegisterRequest request);
    (string accessToken, string refreshToken) LoginUser(LoginRequest request);
    (string accessToken, string refreshToken) RefreshToken(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthService(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public void RegisterUser(RegisterRequest request)
    {
        var (hashedPassword, salt) = SecurityHelpers.GetHashedPasswordAndSalt(request.Password);

        var userToAdd = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Salt = salt,
            Password = hashedPassword
        };

        _context.Users.Add(userToAdd);
        _context.SaveChanges();
    }

    public (string accessToken, string refreshToken) LoginUser(LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(e =>
            string.Equals(request.Email.ToLower(), e.Email.ToLower()));

        if (user == null) throw new Exception("User not found");

        var hashedPassword = SecurityHelpers.GetHashedPasswordWithSalt(request.Password, user.Salt);
        if (hashedPassword != user.Password) throw new Exception("Incorrect password");

        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Customer")
        };

        var accessToken = GenerateAccessToken(userClaims);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExp = DateTime.UtcNow.AddDays(30);
        _context.SaveChanges();

        return (accessToken, refreshToken);
    }

    public (string accessToken, string refreshToken) RefreshToken(string refreshToken)
    {
        var user = _context.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
        if (user == null) throw new Exception("Invalid refresh token");
        if (user.RefreshTokenExp < DateTime.Now) throw new Exception("Refresh token expired");

        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Convert.ToString(user.Id)),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Customer")
        };

        var accessToken = GenerateAccessToken(userClaims);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExp = DateTime.UtcNow.AddDays(30);
        _context.SaveChanges();

        return (accessToken, newRefreshToken);
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var sskey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Auth:SecretKey"]));
        var credentials = new SigningCredentials(sskey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Auth:ValidIssuer"],
            audience: _configuration["Auth:ValidAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}