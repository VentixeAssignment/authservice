using Azure.Core;
using Microsoft.Extensions.Configuration;
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

        var audiences = _config.GetSection("Jwt:Audience").Get<string[]>();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!)
        };

        foreach (var aud in audiences)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = credentials,
            Issuer = _config["Jwt:Issuer"]
        };


        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}
