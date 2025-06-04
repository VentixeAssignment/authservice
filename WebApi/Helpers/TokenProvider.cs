using Azure.Core;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebApi.Entities;
using WebApi.Protos;

namespace WebApi.Helpers;

public sealed class TokenProvider(IConfiguration config)
{
    private readonly IConfiguration _config = config;

    // Created method with help from ChatGpt and Milan Jovanovic on youtube to create a tokenprovider to use for authentication.
    public string CreateToken(UserEntity user)
    {
        var key = _config["Jwt:Key"];
        var encodedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));

        var credentials = new SigningCredentials(encodedKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = credentials,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}
