using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Backend;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class JwtTokenService(IConfigurationRoot root) : ITokenService
{
    private readonly IConfigurationRoot _config = root;
    public string GenerateToken()
    {
        var key = _config.GetSection("Authentication:Key").Value!;
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddHours(6),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}