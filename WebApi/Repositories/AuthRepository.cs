using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using WebApi.Data;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Repositories;

public class AuthRepository
{
    protected readonly DataContext _context;
    private readonly DbSet<UserEntity> _table;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<AuthRepository> _logger;
    private IDbContextTransaction _transaction;

    public AuthRepository(DataContext context, DbSet<UserEntity> table, SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, ILogger<AuthRepository> logger, IDbContextTransaction transaction)
    {
        _context = context;
        _table = context.Set<UserEntity>();
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _transaction = transaction;
    }

    #region CRUD
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
#endregion


    #region Transactions

    public async Task<AuthResult> BeginTransactionAsync()
    {
        if (_transaction != null)
            return new AuthResult { Success = false, ErrorMessage = "Failed to begin transaction because a transaction is already started." };

        try
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "\nFailed to start new transaction for entity of type {EntityType}", typeof(UserEntity).Name);
            return new AuthResult { Success = false, ErrorMessage = $"Something went wrong when starting transation." };
        }
    }

    public async Task<AuthResult> CommitTransactionAsync()
    {
        if (_transaction == null)
            return new AuthResult { Success = false, ErrorMessage = "Another transaction is already in use." };

        try
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null!;

            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            await _transaction.RollbackAsync();
            _logger.LogError(ex.Message, "Failed to commit transaction for entity of type {EntityType}", typeof(UserEntity).Name);
            return new AuthResult { Success = false, ErrorMessage = $"Something went wrong when committing transation." };
        }
    }

    public async Task<AuthResult> RollbackTransactionAsync()
    {
        if (_transaction == null)
            return new AuthResult { Success = false, ErrorMessage = "There is no transaction to roll back." };

        try
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null!;

            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            _transaction = null!;
            _logger.LogError(ex.Message, "Failed to roll back transaction for entity of type {EntityType}", typeof(UserEntity).Name);
            return new AuthResult { Success = false, ErrorMessage = "Failed to rollback transaction." };
        }
    }

    public async Task<AuthResult> SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();

            return new AuthResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Failed to save changes for entity of type {EntityType}", typeof(UserEntity).Name);
            return new AuthResult { Success = false, ErrorMessage = "Failed to save changes." };
        }
    }

    #endregion
}
