
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApplication2.Models;
using System.IO;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.FileIO;
using System.Security.Principal;
using System.Globalization;
using CsvHelper;

namespace WebApplication2.Controllers
{

    public class CSVUploadController : Controller
    {
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(CSVUploadViewModel model)
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

                    //Lecture d'un fichier csv
                    List<string> csvadres = new List<string>();
                    List<string> nballerretour = new List<string>();
                    List<string> locomotion = new List<string>();
                    using (TextFieldParser parser = new TextFieldParser(filePath))
                    {
                        parser.Delimiters = new string[] { "," };
                        while (true)
                        {
                            string[] parts = parser.ReadFields();
                            if (parts == null)
                            {
                                break;
                            }
                            csvadres.Add(parts[0]);
                            nballerretour.Add(parts[1]);
                            locomotion.Add(parts[2]);

                        }
                    }
                    //Suppression de l'entête du CSV
                    csvadres.RemoveAt(0);
                    nballerretour.RemoveAt(0);
                    locomotion.RemoveAt(0);
                    //Calcul des émissions par moyen de transport
                    var Voiture = new FacteurEmission { EqCO2 = 0.231, MoyenDeTransport = "Voiture"};
                    var TrainGrandeLigne = new FacteurEmission { MoyenDeTransport = "TrainGrandeLigne", EqCO2 = 5.92e-3 };
                    var MetroTram = new FacteurEmission { EqCO2 = 5.03e-3, MoyenDeTransport = "MetroTram" };
                    var TER = new FacteurEmission { MoyenDeTransport = "TER", EqCO2 = 0.0277 };
                    var TGV = new FacteurEmission { EqCO2 = 2.93e-3, MoyenDeTransport = "TGV" };
                    var VeloElectrique = new FacteurEmission { EqCO2 = 0.0109, MoyenDeTransport = "VeloElectrique" };
                    var Scooter = new FacteurEmission { MoyenDeTransport = "Scooter", EqCO2 = 0.0763 };
                    var Autobous = new FacteurEmission { MoyenDeTransport = "Autobus", EqCO2 = 0.151 };
                    var Moto = new FacteurEmission { EqCO2 = 0.0763, MoyenDeTransport = "Moto" };

                    var ListMoyenTransport = new List<FacteurEmission>();
                    ListMoyenTransport.Add(TER);
                    ListMoyenTransport.Add(TGV);
                    ListMoyenTransport.Add(Voiture);
                    ListMoyenTransport.Add(Moto);
                    ListMoyenTransport.Add (VeloElectrique);
                    ListMoyenTransport.Add(Autobous);
                    ListMoyenTransport.Add(TrainGrandeLigne);
                    ListMoyenTransport.Add(Scooter);
                    ListMoyenTransport.Add(MetroTram);

                    //Calcul des données
                    string adresseent = model.AdresseEntreprise;
                    string cleApi = model.CleApi;
                    using HttpClient client = new HttpClient();
                    List<double> distances = new List<double>();
                    List<ResultatDetaille> resultatDetailles = new List<ResultatDetaille>();
                    int i = 0;
                    foreach (string s in csvadres)
                    {
                        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?units=metric&origins={adresseent}&destinations={s}&key={cleApi}";

                        // Appel de l'API
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            // Lecture de la réponse JSON
                            string jsonString = await response.Content.ReadAsStringAsync();
                            int indexDebut = jsonString.IndexOf("value");
                            string d = jsonString.Substring(indexDebut + 9);
                            int indexFin = d.IndexOf("\n");
                            string dis = d.Remove(indexFin);
                            double distance = int.Parse(dis)/1000;
                            int nbar = int.Parse(nballerretour[i]);

                            var eqCO2 = ListMoyenTransport.Where(a => a.MoyenDeTransport == locomotion[i]).Select(a => a.EqCO2).FirstOrDefault();
                            
                            var resultatdetaille = new ResultatDetaille
                            {
                                AdresseEmploye = s,
                                NombreAllerRetour = nbar,
                                MoyenDeTransport = locomotion[i],
                                EqCO2 = eqCO2,
                                DistanceUnitaire = distance,
                                DistanceAnnuelle = distance*nbar,
                                EmissionAnuelle = distance*nbar*eqCO2
                            };
                            resultatDetailles.Add(resultatdetaille);

                            i++;

                            ////A supprimer après la mise en place du CSV
                            //double loc = double.Parse(locomotion[i]);
                            //double distancea = distance * nbar * loc;
                            //distances.Add(distancea);
                            
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
                        csv.WriteRecords(resultatDetailles);
                        writer.Flush();
                        var byteArray = memoryStream.ToArray();
                        return File(byteArray, "text/csv", "file.csv");
                    }

                    //A supprimer avec l'ajout du CSV ? 
                    //Calcul de la sommme des distances 
                    double distanceglobale = 0;
                    foreach (var s in distances)
                    {
                        distanceglobale = distanceglobale + s;
                    }
                    int total = (int)Math.Round(distanceglobale);
                    TempData["CSVContent"] = total/1000;
                    // Passe le contenu du fichier à la vue de succès
                    return RedirectToAction("UploadSuccess");
                }
                ModelState.AddModelError("", "File upload failed.");
            }
            return View(model);
        }

        public IActionResult UploadSuccess()
        {
            ViewBag.CSVContent = TempData["CSVContent"];
            return View();
        }

    }

    public class FacteurEmission
    {

        public string MoyenDeTransport { get; set; }
        //kg éq. CO2/km
        public double EqCO2 { get; set; }
    }
    public class ResultatDetaille
    {
        public string AdresseEmploye { get; set; }
        public string MoyenDeTransport { get; set; }
        //kg éq. CO2/km
        public double EqCO2 { get; set; }
        public double DistanceUnitaire { get; set; }
        public int NombreAllerRetour { get; set; }
        public double DistanceAnnuelle { get; set; }
        public double EmissionAnuelle { get; set; }


    }
}
