using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Services;

public interface IAuthService 
{
    Task<AuthResult> SignInAsync(string username, string password);
    Task<AuthResult> SignOutAsync();
}
