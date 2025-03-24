using Backend.Data;
using Microsoft.AspNetCore.Identity;

namespace Utils;

public static class Startup
{
    public static void CreateAdmin(this WebApplication app)
    {   
        var scope = app.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var hasher = new PasswordHasher<string>();
        var passwordHash = hasher.HashPassword("admin", "admin");
        repository.CreateAdminAsync("admin", passwordHash);
    }
}
