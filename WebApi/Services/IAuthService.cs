using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Services;

public interface IAuthService 
{
    Task<AuthResult> SignIn(string username, string password);
    AuthResult SignOut(UserEntity user);
}
