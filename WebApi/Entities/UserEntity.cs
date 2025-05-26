using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Entities
{
    public class UserEntity : IdentityUser
    {
        [PersonalData]
        [Required]
        public string FirstName { get; set; } = null!;

        [PersonalData]
        [Required]
        public string LastName { get; set; } = null!;
    }
}
