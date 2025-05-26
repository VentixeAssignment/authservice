using Grpc.Core;
using WebApi.Data;
using WebApi.Entities;
using WebApi.Protos;
using WebApi.Repositories;

namespace WebApi.Services;

public class AuthServiceGrpc(AuthRepository authRepository, ILogger<AuthService> logger, DataContext context) : AuthHandler.AuthHandlerBase
{
    private readonly AuthRepository _authRepository = authRepository;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly DataContext _context = context;


    public override async Task<CreateReply> CreateUser(CreateRequest request, ServerCallContext context)
    {
            if(string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password)
            )
            return new CreateReply { Success = false, StatusCode = 400, Message = "Not all fields are valid." };

        var newUser = new UserEntity
        {
            UserName = request.Email,
            Email = request.Email
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var result = await _authRepository.CreateUserAsync(newUser, request.Password);

            if (!result.Success)
            {
                await transaction.RollbackAsync();
                return new CreateReply { Success = false, StatusCode = 500, Message = $"Unable to create user with email {request.Email}\nError: {result.ErrorMessage}" };
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CreateReply { Success = true, StatusCode = 201, Message = "User was successfully created." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened trying to create user with email {request.Email}.\n{ex}\n{ex.Message}");
            return new CreateReply { Success = false, StatusCode = 500, Message = $"Unable to create user with email {request.Email}\nError: {ex.Message}" };
        }
    }

    public override async Task<UpdateReply> UpdateUser(UpdateRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) ||
           string.IsNullOrWhiteSpace(request.Email)
           )
            return new UpdateReply { Success = false, StatusCode = 400, Message = "Not all fields are valid." };

        var newUser = new UserEntity
        {
            Id = request.Id,
            UserName = request.Email,
            Email = request.Email
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = await _authRepository.UpdateUserAsync(newUser);

            if (!result.Success)
            {
                await transaction.RollbackAsync();
                return new UpdateReply { Success = false, StatusCode = 500, Message = $"Unable to update user.\nError: {result.ErrorMessage}" };
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new UpdateReply { Success = true, StatusCode = 200, Message = "User was successfully updated." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened trying to update user.\n{ex}\n{ex.Message}");
            return new UpdateReply { Success = false, StatusCode = 500, Message = $"Unable to update user.\nError: {ex.Message}" };
        }
    }

    public override async Task<PasswordReply> UpdatePassword(PasswordRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) ||
            string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword)
           )
            return new PasswordReply { Success = false, StatusCode = 400, Message = "Not all fields are valid." };

        var userResult = await _authRepository.GetUserAsync(request.Id);

        if (!userResult.Success || userResult.Data == null)
            return new PasswordReply { Success = false, StatusCode = 400, Message = $"No user with id {request.Id} was found." };

        var newUser = new UserEntity
        {
            Id = request.Id,
            UserName = userResult.Data.Email,
            Email = userResult.Data.Email
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = await _authRepository.UpdatePasswordAsync(newUser, request.CurrentPassword, request.NewPassword);

            if (!result.Success)
            {
                await transaction.RollbackAsync();
                return new PasswordReply { Success = false, StatusCode = 500, Message = $"Unable to update password.\nError: {result.ErrorMessage}" };
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new PasswordReply { Success = true, StatusCode = 200, Message = "User was successfully updated." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened trying to update password.\n{ex}\n{ex.Message}");
            return new PasswordReply { Success = false, StatusCode = 500, Message = $"Unable to update password.\nError: {ex.Message}" };
        }
    }

    public override async Task<DeleteReply> DeleteUser(DeleteRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            return new DeleteReply { Success = false, StatusCode = 400, Message = "Id cannot be null." };

        var userResult = await _authRepository.GetUserAsync(request.Id);

        if (!userResult.Success || userResult.Data == null)
            return new DeleteReply { Success = false, StatusCode = 400, Message = $"No user with id {request.Id} was found." };

        var user = new UserEntity
        {
            Id = request.Id,
            UserName = userResult.Data.Email,
            Email = userResult.Data.Email
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = await _authRepository.DeleteUserAsync(user);

            if (!result.Success)
                return new DeleteReply { Success = false, StatusCode = 500, Message = result.ErrorMessage };

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new DeleteReply { Success = true, StatusCode = 200, Message = "Successfully deleted user." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened trying to delete user.\n{ex}\n{ex.Message}");
            return new DeleteReply { Success = false, StatusCode = 500, Message = $"Unable to delete user.\nError: {ex}\n{ex.Message}" };
        }
    }
}
