# Travel
Besoin d'intégrer un fichier CSV au format suivant : 
Adresse de l'employé, Nb Aller Retour dans l'année, Type de Moyen de transport

Les moyens de transports sont a séléctionner dans la liste suivante : 
                    var Voiture = new FacteurEmission { EqCO2 = 0.231, MoyenDeTransport = "Voiture"};
                    var TrainGrandeLigne = new FacteurEmission { MoyenDeTransport = "TrainGrandeLigne", EqCO2 = 5.92e-3 };
                    var MetroTram = new FacteurEmission { EqCO2 = 5.03e-3, MoyenDeTransport = "MetroTram" };
                    var TER = new FacteurEmission { MoyenDeTransport = "TER", EqCO2 = 0.0277 };
                    var TGV = new FacteurEmission { EqCO2 = 2.93e-3, MoyenDeTransport = "TGV" };
                    var VeloElectrique = new FacteurEmission { EqCO2 = 0.0109, MoyenDeTransport = "VeloElectrique" };
                    var Scooter = new FacteurEmission { MoyenDeTransport = "Scooter", EqCO2 = 0.0763 };
                    var Autobous = new FacteurEmission { MoyenDeTransport = "Autobus", EqCO2 = 0.151 };
                    var Moto = new FacteurEmission { EqCO2 = 0.0763, MoyenDeTransport = "Moto" };
