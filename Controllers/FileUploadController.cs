using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication2.Models;
using static WebApplication2.Controllers.HomeController;
using CsvHelper;

namespace WebApplication2.Controllers
{
    public class FileUploadController : Controller
    {
        private static List<UploadSuccessViewModel> distances = new List<UploadSuccessViewModel>();
        
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.UploadedFile != null && model.UploadedFile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", model.UploadedFile.FileName);
                    // Sauvegarde du fichier sur le serveur
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                         model.UploadedFile.CopyTo(stream);
                    }
                    // Lecture du contenu du fichier
                    List<string> adresseEmploye = new List<string>();
                    using (var sr = new StreamReader(filePath))
                    {
                        while (sr.Peek() >= 0)
                        {
                            adresseEmploye.Add(sr.ReadLine());
                        }
                    }
                    //Calcul des données
                    string adresseent = model.AdresseEntreprise;
                    string cleApi = model.CleApi;
                    using HttpClient client = new HttpClient();
                    int distanceglobale = 0;

                    //Création d'une liste de résultat
                    var resultat = new List<Resultats>();



                    foreach (string s in adresseEmploye)
                    {
                        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?units=metric&origins={adresseent}&destinations={s}&key={cleApi}";
                        // Appel de l'API
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            // Lecture de la réponse JSON
                            string jsonString =  await response.Content.ReadAsStringAsync();
                            int indexDebut = jsonString.IndexOf("value");
                            string d = jsonString.Substring(indexDebut + 9);
                            int indexFin = d.IndexOf("\n");
                            string dis = d.Remove(indexFin);
                            int distance = int.Parse(dis);
                            //Ajouter les adresses ainsi que les distances dans les listes de classes, n'est plus utile ave l'arrivée du téléchargement du csv
                            var vm =new UploadSuccessViewModel { Adresse = s, Distance = (distance/1000) };
                            distances.Add(vm);
                            distanceglobale = distanceglobale + distance;

                            //Dans la boucle on ajoute le résultat a notre liste
                            var r = new Resultats { Distance = (distance / 1000), AdresseEmploye = s };
                            resultat.Add(r);


                        }
                        else
                        {
                            Console.WriteLine("");
                        }
                    }

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


                    //TempData["FileContent"] = distanceglobale/1000;
                    // //Passe le contenu du fichier à la vue de succès
                    //return RedirectToAction("UploadSuccess") ;
                }
                ModelState.AddModelError("", "File upload failed.");
            }
            return View(model);
        }

        public IActionResult UploadSuccess()
        {
            
            ViewBag.FileContent = TempData["FileContent"];
            return View(distances);
        }
    }


}
