using System.Text;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Installers;

public static class JwtInstaller
{
    public static void AddJwtAuthorization(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var key = configuration.GetSection("Authorization:Key").Value;
        key ??= Environment.GetEnvironmentVariable("AUTH_KEY");
        if (key == null)
            throw new Exception("No authorization key found.");

        var keyBytes = Encoding.UTF8.GetBytes(key);
        services.AddScoped<ITokenService>(sp => new JwtTokenService(key));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            };
        });
        services.AddAuthorization();
    }
}