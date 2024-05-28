using System.Text;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Installers;

public static class JwtInstaller
{
    public static void AddJwtAuthorization(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfigurationRoot>();
        var key = configuration.GetSection("Authentication:Key").Value!;
        var keyBytes = Encoding.UTF8.GetBytes(key);
        services.AddScoped<ITokenService, JwtTokenService>();

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