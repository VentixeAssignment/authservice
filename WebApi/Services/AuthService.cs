using Microsoft.AspNetCore.Identity;
using WebApi.Entities;
using WebApi.Models;
using WebApi.Protos;
using WebApi.Repositories;

namespace WebApi.Services;

public class AuthService(AuthRepository authRepository) : IAuthService
{
    private readonly AuthRepository _authRepository = authRepository;

    public async Task<AuthResult> SignInAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return new AuthResult { Success = false, ErrorMessage = "User name and password cannot be empty." };

        try
        {
            var result = await _authRepository.SignInAsync(userName, password);
            if (result.Success)
                return new AuthResult { Success = true, Message = "User was successfully signed in." };

            return new AuthResult { Success = false, ErrorMessage = "Unable to sign in user." };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = "An unexpected error occurred." };
        }
    }

    public async Task<AuthResult> SignOutAsync()
    {
        var result = await _authRepository.SignOutAsync();
        return new AuthResult { Success = result.Success, Message = result.Message ?? "", ErrorMessage = result.ErrorMessage ?? "" };
    }    
}
