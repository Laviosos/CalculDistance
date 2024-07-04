using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Globalization;
using WebApplication2.Models;
using CsvHelper;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
           
            //new TextFieldParser file = TextFieldParser

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public class Resultats
        {
            
            public string AdresseEmploye { get; set; }
            public int Distance { get; set; }
        }
        public IActionResult ExtractionCSV()
        {
            //Création d'une liste de résultat
            var resultat = new List<Resultats>
            {
                new Resultats { Distance = 1, AdresseEmploye = "one" },
                 new Resultats { Distance = 2, AdresseEmploye = "two" },
            };

            //Dans la boucle on ajoute le résultat a notre liste


            //On ajoute ensuite tous les résultats au csv
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(resultat);
                writer.Flush();
                var byteArray = memoryStream.ToArray();
                return File(byteArray, "text/csv", "file.csv");
            }
        }

    }
}
