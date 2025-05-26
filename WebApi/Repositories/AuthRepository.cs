using Microsoft.AspNetCore.Identity;
using WebApi.Data;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Repositories;

public class AuthRepository(DataContext context, SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, ILogger<AuthRepository> logger)
{
    protected readonly DataContext _context = context;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly ILogger<AuthRepository> _logger = logger;


    public async Task<AuthResult> SignInAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return new AuthResult { Success = false, ErrorMessage = "User name and password can not be empty." };

        try
        {
            var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);

            if (result.Succeeded)
                return new AuthResult { Success = true };

            return new AuthResult { Success = false, ErrorMessage = "Failed to sign in user." };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = $"Something went wrong signing in user.\n{ex}\n{ex.Message}" };
        }
    }

    public async Task<AuthResult> SignOut()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = $"Something went wrong signing out user.\n{ex}\n{ex.Message}" };
        }
    }


    public async Task<AuthResult> CreateUserAsync(UserEntity user, string password)
    {
        if (user == null || string.IsNullOrWhiteSpace(password))
            return new AuthResult { Success = false, ErrorMessage = "User and password cannot be null or empty." };

        try
        {
            var result = await _userManager.CreateAsync(user, password);

            return result.Succeeded
                ? new AuthResult { Success = true, Message = "User was successfully created." }
                : new AuthResult { Success = false, ErrorMessage = "Unable to create user." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something went wrong creating user with usermanager. {ex.Message}");
            return new AuthResult { Success = false, ErrorMessage = $"Failed to create user. {ex.Message}" };
        }
    }
}
