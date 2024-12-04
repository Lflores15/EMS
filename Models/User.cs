using Microsoft.AspNetCore.Identity;

namespace EMS.Models
{
    public class User : IdentityUser
    {
        public bool IsApproved { get; set; } // Default is not approved
    }
}

