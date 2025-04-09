using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Utils;

public static class Startup
{
    public static async Task CreateAdmin(this WebApplication app)
    {
        var username = app.Configuration["Admin:Username"];
        var password = app.Configuration["Admin:Password"];
        username ??= Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        password ??= Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if(username == null || password == null)
            throw new Exception("Admin username or password not found.");

        var scope = app.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        try
        {
            var admin = await repository.GetAdminAsync(username);
        }
        catch(Exception)
        {
            var hasher = new PasswordHasher<string>();
            var passwordHash = hasher.HashPassword(username, password);

            var newAdmin = new Admin{Username = username, PasswordHash = passwordHash};
            await repository.CreateAdminAsync(newAdmin);
        }
    }
}
