using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;

namespace WebApi.Entities;

public class UserEntity : IdentityUser
{
    public bool IsActive { get; set; } = true;
}
