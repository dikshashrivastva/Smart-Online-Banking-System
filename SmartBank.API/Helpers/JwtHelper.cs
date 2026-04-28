using Microsoft.IdentityModel.Tokens;
using SmartBank.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartBank.API.Helpers;

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier,     user.UserId.ToString()),
            new Claim(ClaimTypes.Email,              user.Email),
            new Claim(ClaimTypes.Role,               user.Role?.RoleName ?? "Customer"),
            new Claim("firstName",                   user.FirstName),
            new Claim("lastName",                    user.LastName),
        };

        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds     = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMin = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440");

        var token = new JwtSecurityToken(
            issuer:             jwtSettings["Issuer"],
            audience:           jwtSettings["Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiryMin),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
