using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;


    public async Task<IActionResult> SignInAsync(string userName, string password)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            return BadRequest("User name and password cannot be empty.");

        var result = await _authService.SignInAsync(userName, password);

        return result.Success ? Ok(result) : BadRequest("Unable to sign in.");
    }

    public async Task<IActionResult> SignOutAsync()
    {
        var result = await _authService.SignOutAsync();

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }
}
