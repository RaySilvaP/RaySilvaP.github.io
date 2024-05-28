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
        if (admin == null)
            return false;

        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(login.Username, admin.PasswordHash, login.Password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}