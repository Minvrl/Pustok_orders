using System.ComponentModel.DataAnnotations;

namespace MVC_Pustok.ViewModels
{
    public class ProfileEditViewModel
    {
        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        public string UserName { get; set; }
        [Required]
        [MinLength(10)]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(35)]
        public string FullName { get; set; }
        [MinLength(8)]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        
        [MinLength(8)]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        [MinLength(8)]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string? ConfirmNewPassword { get; set; }
    }
}
