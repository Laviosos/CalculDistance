﻿using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class FileUploadViewModel
    {
        [Required]
        [Display(Name = "Upload File")]
        public IFormFile UploadedFile { get; set; }
        public string CleApi { get; set; }
        public string AdresseEntreprise { get; set; }
    }

}
