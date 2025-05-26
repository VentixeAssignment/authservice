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
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password)
            )
            return new CreateReply { Success = false, StatusCode = 400, Message = "Not all fields are valid." };

        var newUser = new UserEntity
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
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
            _logger.LogWarning($"Something unexpected went wrong trying to create user with email {request.Email}.\n{ex}\n{ex.Message}");
            return new CreateReply { Success = false, StatusCode = 500, Message = $"Unable to create user with email {request.Email}\nError: {ex.Message}" };
        }
    }

    public override Task<UpdateReply> UpdateUser(UpdateRequest request, ServerCallContext context)
    {
        return base.UpdateUser(request, context);
    }

    public override Task<DeleteReply> DeleteUser(DeleteRequest request, ServerCallContext context)
    {
        return base.DeleteUser(request, context);
    }
}
