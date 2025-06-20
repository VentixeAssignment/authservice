﻿using Grpc.Core;
using WebApi.Data;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Protos;
using WebApi.Repositories;

namespace WebApi.Services;

public class AuthServiceGrpc(AuthRepository authRepository, ILogger<AuthService> logger, DataContext context, IAuthService authService) : AuthHandler.AuthHandlerBase
{
    private readonly AuthRepository _authRepository = authRepository;
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly DataContext _context = context;



    public override async Task<SigninReply> SignIn(SigninRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new SigninReply { Token = "", Message = "SigninRequest is invalid.", Succeeded = false };
        try
        {
            var result = await _authService.SignInAsync(request.Email, request.Password);
            
            return result.Success
                ? new SigninReply { Token = result.Token, Message = "Signin successfull.", Succeeded = true }
                : new SigninReply { Message = $"Unable to sign in. {result.ErrorMessage}", Succeeded = false };

        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Something went wrong when signing in. ##### {ex.Message}");
            return new SigninReply { Token = "", Message = $"Something unexpected happened when signing in. {ex}", Succeeded = false };
        }
    }


    public override async Task<CreateReply> CreateUser(CreateRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
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

            var newEntity = await _authRepository.GetUserAsync(null, request.Email);

            return new CreateReply { Success = true, StatusCode = 201, Message = "User was successfully created.", UserId = newEntity.Data?.Id ?? "" };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened trying to create user with email {request.Email}.\n{ex}\n{ex.Message}");
            return new CreateReply { Success = false, StatusCode = 500, Message = $"Unable to create user with email {request.Email}\nError: {ex.Message}" };
        }
    }


    public override async Task<ExistsReply> AlreadyExists(ExistsRequest request, ServerCallContext context)
    {
        if (request == null)
            return new ExistsReply { Success = false, StatusCode = 400, Message = "Invalid request." };

        var result = await _authRepository.AlreadyExistsAsync(request.Email);

        return result.Success
            ? new ExistsReply { Success = true, StatusCode = 409, Message = $"User with email {request.Email} already exists." }
            : new ExistsReply { Success = false, StatusCode = 200, Message = $"User with email {request.Email} does not exist. " };
    }


    public override async Task<EmailReply> GetUserEmail(EmailRequest request, ServerCallContext context)
    {
        if (request == null)
            return new EmailReply { Success = false, Message = "Email can not be empty." };

        var entity = await _authRepository.GetUserAsync(request.Id, null);

        if (entity == null)
            return new EmailReply { Success = false, Message = $"No user found with id: {request.Id}" };

        return new EmailReply { Success = true, Email = entity.Data!.Email };
    }


    public override async Task<VerifyCodeReply> VerifyCode(VerifyCodeRequest request, ServerCallContext context)
    {
        if(request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code) || !request.Email.Contains('@'))
            return new VerifyCodeReply { Succeeded = false, StatusCode = 400, Message = "Invalid request. Data is missing or invalid." };

        var user = await _authRepository.GetUserAsync(null, request.Email);

        if(user == null || user.Data == null)
            return new VerifyCodeReply { Succeeded = false, StatusCode = 404, Message = $"User with email {request.Email} was not found." };

        user.Data.EmailConfirmed = true;
        var updateResult = await _authRepository.UpdateUserAsync(user.Data);

        if(!updateResult.Success) 
            return new VerifyCodeReply { Succeeded = false, StatusCode = 500, Message = updateResult.Message };

        return new VerifyCodeReply { Succeeded = true, StatusCode = 200 };
    }


    public override async Task<UpdateReply> UpdateUser(UpdateRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id) ||
           string.IsNullOrWhiteSpace(request.Email)
           )
            return new UpdateReply { Success = false, StatusCode = 400, Message = "Not all fields are valid." };

        //var exists = 

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

        var userResult = await _authRepository.GetUserAsync(request.Id, null);

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


    public override async Task<ActiveReply> ChangeActive(ActiveRequest request, ServerCallContext context)
    {
        if (request == null)
            return new ActiveReply { Success = false, StatusCode = 400, Message = "Invalid request." };

        var userResult = await _authRepository.GetUserAsync(request.Id, null);

        if (!userResult.Success || userResult.Data == null)
            return new ActiveReply { Success = false, StatusCode = 404, Message = $"No user with id {request.Id} was found." };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await _authRepository.UpdateUserAsync(userResult.Data);
            if (!result.Success)
            {
                await transaction.RollbackAsync();
                return new ActiveReply { Success = false, StatusCode = 500, Message = $"Unable to update user status.\nError: {result.ErrorMessage}" };
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ActiveReply { Success = true, StatusCode = 200, Message = "User status was successfully updated." };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Something unexpected happened trying to update user status.\n{ex}\n{ex.Message}");
            return new ActiveReply { Success = false, StatusCode = 500, Message = $"Unable to update user status.\nError: {ex.Message}" };
        }
    }


    public override async Task<DeleteReply> DeleteUser(DeleteRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
            return new DeleteReply { Success = false, StatusCode = 400, Message = "Id cannot be null." };

        var userResult = await _authRepository.GetUserAsync(request.Id, null);

        if (!userResult.Success || userResult.Data == null)
            return new DeleteReply { Success = false, StatusCode = 404, Message = $"No user with id {request.Id} was found." };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var result = await _authRepository.DeleteUserAsync(userResult.Data);

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
