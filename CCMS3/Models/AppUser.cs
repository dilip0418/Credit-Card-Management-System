using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCMS3.Models
{
    public class AppUser : IdentityUser
    {
        [Column(TypeName = "nvarchar(150)")]
        public string FullName { get; set; }

        public string? ActivationCode { get; set; }
        public DateTime? CodeExpiration { get; set; }
        public bool IsActive { get; set; } = false;
        public PersonalDetails? PersonalDetails { get; set; }
    }
}
