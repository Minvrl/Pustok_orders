using Microsoft.AspNetCore.Identity;

namespace MVC_Pustok.Models
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; }    
    }
}
