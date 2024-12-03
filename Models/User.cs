using Microsoft.AspNetCore.Identity;

namespace EMS.Models
{
    public class User : IdentityUser
    {
    // You can add other properties here, like a flag to track approval status
        public bool IsApproved { get; set; } // Default is not approved
    }
}

