using Backend.Data;
using Microsoft.AspNetCore.Identity;

namespace Backend.Utils;

public static class Startup
{
    public static async Task CreateAdmin(this WebApplication app)
    {   
        var username = "admin";
        var password = "admin";

        var scope = app.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        
        var admin = await repository.GetAdminAsync(username);
        if(admin != null)
            return;

        var hasher = new PasswordHasher<string>();
        var passwordHash = hasher.HashPassword(username, password);

        await repository.CreateAdminAsync("admin", passwordHash);
    }
}
