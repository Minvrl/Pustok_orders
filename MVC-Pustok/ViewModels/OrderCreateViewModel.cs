using System.ComponentModel.DataAnnotations;

namespace MVC_Pustok.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required]
        [MaxLength(200)]
        public string Adress {  get; set; }
        [Required]
        [MaxLength(40)]
        public string Phone { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
