using MVC_Pustok.Models;

namespace MVC_Pustok.ViewModels
{
    public class ProfileViewModel
    {
        public ProfileEditViewModel ProfEditVM { get; set; }
        public List<Order> Orders { get; set; } 
    }
}
