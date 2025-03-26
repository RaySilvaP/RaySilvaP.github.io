using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services;

public class CredentialsService(IRepository repository)
{
    private readonly IRepository _repository = repository;

    public async Task<bool> VerifyCredentialsAsync(Login login)
    {
        var admin = await _repository.GetAdminAsync(login.Username);

        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(login.Username, admin.PasswordHash, login.Password);
        if(result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            admin.PasswordHash = hasher.HashPassword(login.Username, login.Password);
            await _repository.UpdateAdminPassword(admin);
            return true;
        }
        return result == PasswordVerificationResult.Success;
    }
}
