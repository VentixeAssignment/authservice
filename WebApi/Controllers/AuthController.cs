using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Protos;
using WebApi.Services;

namespace WebApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [Route("signin")]
    [HttpPost]
    public async Task<IActionResult> SignInAsync([FromBody] SigninModel model)
    {
        if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            return BadRequest("User name and password cannot be empty.");

        var result = await _authService.SignInAsync(model.UserName, model.Password);

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
