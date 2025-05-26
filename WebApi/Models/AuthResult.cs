using WebApi.Entities;

namespace WebApi.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public UserEntity? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
}
