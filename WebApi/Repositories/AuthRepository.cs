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



    public async Task<AuthResult> SignOutAsync()
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
            _logger.LogWarning($"Something went wrong creating user with usermanager.\n{ex.Message}");
            return new AuthResult { Success = false, ErrorMessage = $"Failed to create user.\n{ex.Message}" };
        }
    }



    public async Task<AuthResult> GetUserAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return new AuthResult { Success = false, ErrorMessage = "Id cannot be null." };

        var result = await _userManager.FindByIdAsync(id);

        return result != null
            ? new AuthResult { Success = true, Data = result }
            : new AuthResult { Success = false, ErrorMessage = $"No user with id {id} was found." };
    }



    public async Task<AuthResult> UpdateUserAsync(UserEntity user)
    {
        if (user == null)
            return new AuthResult { Success = false, ErrorMessage = "User cannot be null." };

        try
        {
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? new AuthResult { Success = true, Message = "User was successfully updated." }
                : new AuthResult { Success = false, ErrorMessage = "Unable to update user." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something went wrong updating user with usermanager.\n{ex.Message}");
            return new AuthResult { Success = false, ErrorMessage = $"Failed to update user.\n{ex.Message}" };
        }
    }



    public async Task<AuthResult> UpdatePasswordAsync(UserEntity user, string currentPassword, string newPassword)
    {
        if (user == null || !string.IsNullOrEmpty(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return new AuthResult { Success = false, ErrorMessage = "User, current password and new password cannot be null or empty." };

        try
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            return result.Succeeded
                ? new AuthResult { Success = true, Message = "Password was successfully updated." }
                : new AuthResult { Success = false, ErrorMessage = "Unable to update password." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something went wrong updating password for user with email {user.Email}. {ex.Message}");
            return new AuthResult { Success = false, ErrorMessage = $"Failed to update password for user with email {user.Email}.\n{ex.Message}" };
        }
    }



    public async Task<AuthResult> DeleteUserAsync(UserEntity user)
    {
        if (user == null)
            return new AuthResult { Success = false, ErrorMessage = "User cannot be null." };

        try
        {
            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded
                ? new AuthResult { Success = true, Message = "User was successfully deleted." }
                : new AuthResult { Success = false, ErrorMessage = "Unable to delete user." };
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something went wrong deleting user with email {user.Email}. {ex.Message}");
            return new AuthResult { Success = false, ErrorMessage = $"Failed to delete user with email {user.Email}.\n{ex.Message}" };
        }
    }
}
