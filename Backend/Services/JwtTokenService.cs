using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Backend;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class JwtTokenService(string key) : ITokenService
{
    public string GenerateToken()
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}