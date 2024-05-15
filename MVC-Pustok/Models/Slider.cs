﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MVC_Pustok.Attributes.ValidationAttributes;

namespace MVC_Pustok.Models
{
    public class Slider:BaseEntity
    {
       
        public string? Title1 { get; set; }
        public string? Title2 { get; set; }
        public string? BtnText { get; set; }
        public string? BtnURL { get; set; }
        public string? Desc { get; set; }
        public string? Image { get; set; }
        public int Order { get; set; }

        [NotMapped]
        [MaxfileSize(1024 * 1024 * 2)]
        [AllowedFileTypes("image/png", "image/jpeg")]
        public IFormFile? ImageFile { get; set; }

    }
}
