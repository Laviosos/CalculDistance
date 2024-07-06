using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class CSVUploadViewModel
    {
        [Required]
        [Display(Name = "Fichier CSV")]
        public IFormFile UploadedFile { get; set; }
        [Display(Name = "Clé API Google Maps")]
        public string CleApi { get; set; }
        [Display(Name = "Adresse de l'entreprise")]
        public string AdresseEntreprise { get; set; }
    }

}
