using Azure.Core;
using Microsoft.AspNetCore.Identity;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Protos;
using WebApi.Repositories;

namespace WebApi.Services;

public class AuthService(AuthRepository authRepository, TokenProvider tokenProvider) : IAuthService
{
    private readonly AuthRepository _authRepository = authRepository;
    private readonly TokenProvider _tokenProvider = tokenProvider;

    public async Task<AuthResult> SignInAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return new AuthResult { Success = false, ErrorMessage = "User name and password cannot be empty." };

        try
        {
            var result = await _authRepository.SignInAsync(userName, password);
            if (result.Success)
            {
                var user = await _authRepository.GetUserAsync(null, userName);

                if (user.Data == null || string.IsNullOrWhiteSpace(user.Data.Id) || string.IsNullOrWhiteSpace(user.Data.Email))
                    return new AuthResult { Success = false, Message = $"No user found with email {userName} to send to TokenProvider." };

                var token = _tokenProvider.CreateToken(user.Data);

                return new AuthResult { Success = true, Message = "User was successfully signed in.", Token = token };
            }

            return new AuthResult { Success = false, ErrorMessage = "Unable to sign in user." };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = $"An unexpected error occurred.\n{ex.Message}" };
        }
    }

    public async Task<AuthResult> SignOutAsync()
    {
        var result = await _authRepository.SignOutAsync();
        return new AuthResult { Success = result.Success, Message = result.Message ?? "", ErrorMessage = result.ErrorMessage ?? "" };
    }
}
