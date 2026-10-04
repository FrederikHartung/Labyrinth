using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public static class ExportImportHelper
    {
        public static bool Erstelle_Unterordner { get; set; } = true;

        private static string default_Speicherort;
        public static string Default_Speicherort
        {
            get
            {
                if (string.IsNullOrEmpty(default_Speicherort))
                {
                    default_Speicherort = Besorge_Default_Speicherort(Erstelle_Unterordner);
                }
                return default_Speicherort;
            }
            set
            {
                default_Speicherort = value;
            }
        }


        /// <summary>
        /// return false, wenn exportObject keine Klasse ist oder keine öffentlichen Properties hat
        /// </summary>
        /// <param name="ordnerPfad_export"></param>
        /// <param name="exportObject"></param>
        /// <returns></returns>
        public static string Exportiere_Objekt_Liste(string ordnerPfad_export, string dateiname, List<Object> objectListe, Type objectType)
        {
            string filepath = $@"{ordnerPfad_export}{dateiname}.txt";
            string hinweis;
            var konvertierteListe = ObjectConverter.CreateList(objectType);
            foreach (var item in objectListe)
            {
                konvertierteListe.Add(item);
            }

            if (konvertierteListe.Count == 0)
            {
                return hinweis = "Die Objektliste zum exportieren enthielt keine Elemente. Daher wurden keine Elemente exportiert";
            }


            #region
            int counter_erfolgreich = 0;
            int counter_nichtErfolgreich = 0;
            int index = 0;
            //string verzeichnis = $"{ordnerPfad_export.Substring(0, ordnerPfad_export.LastIndexOf("\\") + 1)}";
            string fehlerListe_Dateiname = "Fehlerliste.txt";

            Erstelle_OrdnerStruktur_wennNoetig(ordnerPfad_export);
            using (StreamWriter writer = new StreamWriter(ordnerPfad_export))
            {
                //Kopf:
                string projektName = Assembly.GetExecutingAssembly().GetName().Name;
                writer.WriteLine($"Export durch \"{projektName}\" am {DateTime.Now}\r\n");
                writer.WriteLine($"<class: \"{objectType.AssemblyQualifiedName}\">");

                foreach (var item in konvertierteListe) //für jede Instanz eines BO
                {
                    var propertyListe = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance); //besorgen aller Properties
                    if (propertyListe.Length > 0)
                    {
                        List<string> instanz_propertyStringListe = new List<string>();
                        List<string> instanz_propertyStringListe_fehler = new List<string>();
                        bool fehler = false;
                        bool ohneEindeutigeGuid = false;

                        var guidListe = propertyListe.Where(x => x.PropertyType == typeof(Guid));
                        string guidString = "";
                        if (guidListe.Count() == 1)
                        {
                            //instanz_propertyStringListe.Add($"<instanz Index = \"{index}\" >");
                            guidString = $"Guid: \"{guidListe.ElementAt(0).GetValue(item, null)}\"";
                        }
                        else if (guidListe.Count() == 0)
                        {
                            ohneEindeutigeGuid = true;
                            guidString = "Guid: \"keine Guid vorhanden\"";
                            //instanz_propertyStringListe.Add($"<instanz Index = \"{index}\" >");
                        }
                        else if (guidListe.Count() > 1)
                        {
                            ohneEindeutigeGuid = true;
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("mehrere Guids: \"");
                            foreach (var guidProperty in guidListe)
                            {
                                stringBuilder.Append(guidProperty.GetValue(item, null));
                                stringBuilder.Append(", ");
                            }
                            stringBuilder = stringBuilder.Remove(stringBuilder.Length - 2, 2); //um das letzt überschüssige ", " zu entfernen
                            stringBuilder.Append("\"");
                            guidString = stringBuilder.ToString();
                            //instanz_propertyStringListe.Add(stringBuilder.ToString());
                        }
                        instanz_propertyStringListe.Add($"<instanz Index = \"{index}\" {guidString}>");

                        foreach (var property in propertyListe)
                        {
                            string propertyValue = property.GetValue(item, null).ToString();
                            if (propertyValue.Contains("\""))
                            {
                                fehler = true;
                                string ausgabe = $"Klasse: \"{objectType.AssemblyQualifiedName}\" {guidString} Eigenschaft: \"{property.PropertyType}\" Wert: \"{property.GetValue(item, null)}\"";
                                instanz_propertyStringListe_fehler.Add(ausgabe);
                                //continue statt break, um auch die restlichen Properties zu überprüfen. Es werden dann nur die Fehlerhaften ins Fehlerprotokoll geschrieben.
                                continue;
                            }

                            string exportString = $"<Eigenschaft: \"{property.Name}\" Typ: \"{property.PropertyType}\" Wert: \"{propertyValue}\">";
                            instanz_propertyStringListe.Add(exportString);
                        }


                        if (!fehler)
                        {
                            foreach (string line in instanz_propertyStringListe)
                            {
                                writer.WriteLine(line);
                            }
                            counter_erfolgreich++;
                        }
                        else //wenn ein Fehler in den Properties gefunden wurde
                        {
                            string filePath_Fehlerliste = ordnerPfad_export.Insert(ordnerPfad_export.LastIndexOf("\\") + 1, "FehlerlisteExport_");
                            Erstelle_OrdnerStruktur_wennNoetig(filePath_Fehlerliste);

                            using (StreamWriter writer_fehler = new StreamWriter(filePath_Fehlerliste, true))
                            {
                                if (instanz_propertyStringListe_fehler.Count == 1)
                                {
                                    writer_fehler.WriteLine("Folgende Eigenschaft wies einen Fehler auf:");
                                }
                                else if (instanz_propertyStringListe_fehler.Count > 1)
                                {
                                    writer_fehler.WriteLine("Folgende Eigenschaften wiesen einen Fehler auf:");
                                }

                                foreach (string line in instanz_propertyStringListe_fehler)
                                {

                                    writer_fehler.WriteLine(line);
                                }

                                if (ohneEindeutigeGuid)
                                {
                                    if (instanz_propertyStringListe_fehler.Count > 1)
                                    {
                                        writer_fehler.WriteLine("Folgende Eigenschaften waren ok:");
                                    }
                                    else if (instanz_propertyStringListe.Count == 1)
                                    {
                                        writer_fehler.WriteLine("Folgende Eigenschaft war ok:");
                                    }
                                }

                                counter_nichtErfolgreich++;
                            }
                            index++;
                        }
                    }
                    writer.WriteLine("</instanz>"); //Ende des schreibens einer Instanz
                    index++;
                }
                writer.WriteLine($"</class: \"{objectType}\">"); //Ende des schreibens einer Klasse

                if (counter_nichtErfolgreich == 0 && counter_erfolgreich > 0)
                {
                    return hinweis = $"Es wurden {counter_erfolgreich} Datensätze erfolgreich exportiert.";
                }
                else if (counter_nichtErfolgreich > 0 && counter_erfolgreich > 0)
                {
                    hinweis = $"Es wurden {counter_erfolgreich} Datensätze erfolgreich exportiert.\r\n";
                    return hinweis += $"{counter_nichtErfolgreich} Datensätze konnten nicht exportiert werden." +
                        $" Für weitere Infos, überprüfen Sie bitte das Fehlerprotokoll \"{fehlerListe_Dateiname}\" in dem Verzeichnis \"{ordnerPfad_export}\".";
                }
                else if (counter_nichtErfolgreich > 0 && counter_erfolgreich == 0)
                {
                    return hinweis = $"{counter_nichtErfolgreich} Datensätze konnten nicht exportiert werden." +
                     $" Für weitere Infos, überprüfen Sie bitte das Fehlerprotokoll \"{fehlerListe_Dateiname}\" in dem Verzeichnis \"{ordnerPfad_export}\".";
                }
                else
                {
                    return hinweis = $"Es wurden keine Datensätze exportiert.";
                }
                #endregion
            }
        }

        /// <summary>
        /// Rückgabestring = Hinweis, ob alles ok
        /// </summary>
        /// <param name="ordnerPfad_export"></param>
        /// <param name="dateiname"></param>
        /// <param name="properties"></param>
        /// <param name="exportiertes_static_object"></param>
        /// <returns></returns>
        public static (string hinweis, bool fehler) Exportiere_Static_Objekt(string ordnerPfad_export, string dateiname, PropertyInfo[] properties, Type exportiertes_static_object)
        {
            string filepath = $@"{ordnerPfad_export}{dateiname}.txt";
            string filepath_puffer = "";
            //string hinweis;
            bool alteDateiVorhanden = false;

            if (properties.Count() == 0)
            {
                return ("Das Objekt zum exportieren enthielt keine Elemente. Daher wurde das Elemente nicht exportiert", true);
            }

            if (File.Exists(filepath))
            {
                alteDateiVorhanden = true;
                filepath_puffer = filepath;
                filepath = filepath.Insert(filepath.LastIndexOf("."), "_temp");
            }

            int counter_erfolgreich = 0;

            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                //Kopf:
                string projektName = Assembly.GetExecutingAssembly().GetName().Name;
                writer.WriteLine($"Export durch \"{projektName}\" am \"{DateTime.Now}\"\r\n");
                writer.WriteLine($"<class: \"{exportiertes_static_object.AssemblyQualifiedName}\">");

                List<string> instanz_propertyStringListe = new List<string>();
                List<string> instanz_propertyStringListe_fehler = new List<string>();

                PropertyInfo guid = properties.FirstOrDefault(x => x.PropertyType == typeof(Guid));
                if (guid != null)
                {
                    instanz_propertyStringListe.Add($"<instanz = {guid.GetValue(exportiertes_static_object)}>");
                }
                else
                {
                    instanz_propertyStringListe.Add($"<instanz>");
                }

                foreach (PropertyInfo property in properties) //für jede Instanz einer Klasse
                {

                    string propertyValue = property.GetValue(exportiertes_static_object, null).ToString();
                    if (propertyValue.Contains("\""))
                    {
                        propertyValue = propertyValue.Replace("\"", "#&_1"); //WIP: Import: Beim importieren checken und ggf zurück konvertieren
                    }

                    string exportString = $"<Eigenschaft: \"{property.Name}\" Typ: \"{property.PropertyType}\" Wert: \"{propertyValue}\">";

                    writer.WriteLine(exportString);
                    counter_erfolgreich++;

                    #region alt
                    //else //wenn ein Fehler in den Properties gefunden wurde
                    //{
                    //    string filePath_Fehlerliste = ordnerPfad_export.Insert(ordnerPfad_export.LastIndexOf("\\") + 1, "FehlerlisteExport_");
                    //    Erstelle_OrdnerStruktur_wennNoetig(filePath_Fehlerliste);

                    //    using (StreamWriter writer_fehler = new StreamWriter(filePath_Fehlerliste, true))
                    //    {
                    //        if (instanz_propertyStringListe_fehler.Count == 1)
                    //        {
                    //            writer_fehler.WriteLine("Folgende Eigenschaft wies einen Fehler auf:");
                    //        }
                    //        else if (instanz_propertyStringListe_fehler.Count > 1)
                    //        {
                    //            writer_fehler.WriteLine("Folgende Eigenschaften wiesen einen Fehler auf:");
                    //        }

                    //        foreach (string line in instanz_propertyStringListe_fehler)
                    //        {
                    //            writer_fehler.WriteLine(line);
                    //        }

                    //        counter_nichtErfolgreich++;
                    //    }
                    //}
                    #endregion
                }

                instanz_propertyStringListe.Add($"</instanz>");
                writer.WriteLine($"</class: \"{exportiertes_static_object}\">"); //Ende des schreibens einer Klasse
            }

            if (alteDateiVorhanden)
            {
                //Löschen der alten Datei
                File.Delete(filepath_puffer);
                //unbenennen der neuen "_temp" Datei in den vorgesehenen Dateinamen
                System.IO.File.Move(filepath, filepath_puffer);
            }

            #region alt
            //if (counter_nichtErfolgreich == 0 && counter_erfolgreich > 0)
            //{
            //    return (hinweis = $"Es wurde(n) {counter_erfolgreich} Datensätze erfolgreich exportiert.", false);
            //}
            //else if (counter_nichtErfolgreich > 0 && counter_erfolgreich > 0)
            //{
            //    hinweis = $"Es wurden {counter_erfolgreich} Datensätze erfolgreich exportiert.\r\n";
            //    return (hinweis += $"{counter_nichtErfolgreich} Datensätze konnten nicht exportiert werden." +
            //        $" Für weitere Infos, überprüfen Sie bitte das Fehlerprotokoll \"{fehlerListe_Dateiname}\" in dem Verzeichnis \"{ordnerPfad_export}\".", true);
            //}
            //else if (counter_nichtErfolgreich > 0 && counter_erfolgreich == 0)
            //{
            //    return (hinweis = $"{counter_nichtErfolgreich} Datensätze konnten nicht exportiert werden." +
            //     $" Für weitere Infos, überprüfen Sie bitte das Fehlerprotokoll \"{fehlerListe_Dateiname}\" in dem Verzeichnis \"{ordnerPfad_export}\".", true);
            //}
            //else
            //{
            //    return (hinweis = $"Es wurden keine Datensätze exportiert.", false);
            //}
            #endregion

            return ($"Das Objekt \"{exportiertes_static_object.Name}\" wurde mit {counter_erfolgreich} Eigenschaften erfolgreich exportiert.", false);
        }

        /// <summary>
        /// veraltet
        /// </summary>
        /// <param name="speicherort_derDatei"></param>
        public static void Erstelle_OrdnerStruktur_wennNoetig(string speicherort_derDatei)
        {
            //string ordnerPath = StringHelper.Besorge_OrdnerPfad_Von_DateiPfad(filepath_desZuerstellenden_objektes);
            if (!Directory.Exists(speicherort_derDatei))
            {
                Directory.CreateDirectory(speicherort_derDatei);
            }
        }

        public static string Besorge_Default_Speicherort(bool erstelleExportordner)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(AssemblyHelper.BesorgeAssemblyOrdnerPfad());
            if (erstelleExportordner)
            {
                builder.Append("Exporthelper\\");
            }

            string ausgabe = builder.ToString();

            if (!Directory.Exists(ausgabe))
            {
                Directory.CreateDirectory(ausgabe);
            }

            return ausgabe;
        }

        public static string Importiere_Objekt_Liste(string fileName)
        {
            string filePath_Fehlerliste = fileName.Insert(fileName.LastIndexOf("\\") + 1, "FehlerlisteImport_");
            Erstelle_OrdnerStruktur_wennNoetig(filePath_Fehlerliste);
            int counter_importierteDatensaetze = 0;
            int counter_fehlerhafterDatensaetze = 0;
            List<Object> erfolgreiche_importeListe = new List<Object>();
            List<Object> fehlerhafte_importeListe = new List<Object>();

            using (StreamReader reader = new StreamReader(fileName))
            {
                while (true)
                {
                    string line_klasse = reader.ReadLine();
                    if (line_klasse == null)
                    {
                        break;
                    }

                    if (line_klasse.Contains("<class:"))
                    {
                        //"Klassendaten"
                        int posiStart_klasse = line_klasse.IndexOf("\"") + 1;
                        string klassenName = line_klasse.Substring(posiStart_klasse, line_klasse.IndexOf(",") - posiStart_klasse);

                        //Bestimmung der Klasse durch Reflexion
                        var abc = Assembly.Load("ConsoleApp3").GetType("Testnamespace.Kunde");
                        //var abc = Assembly.Load("ConsoleApp3").GetType("Testnamespace.Kunde, ConsoleApp3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                        //var assembly = Assembly.lo
                        string assemblyString = StringHelper.BesorgeSubstring(line_klasse, ",", ",").Trim();
                        Type typ_klasse = Assembly.Load(assemblyString).GetType(klassenName);
                        var neue_instanz = Activator.CreateInstance(typ_klasse);
                        List<string> fehlerhafteZeilen = new List<string>();
                        List<string> erfolgreicheZeilen = new List<string>();
                        List<string> fehlerMeldungen = new List<string>();

                        while (true) //Für jede Instanz dieser Klasse
                        {
                            string line_instanzen = reader.ReadLine();
                            if (line_instanzen.Contains("/class:"))
                            {
                                break;
                            }

                            bool fehlerGefunden = false;
                            // string guidString = ""; //sollte nacher ein Fehler auftreten, wird der String erst gebraucht und gesetzt


                            while (true) //auslesen und zuweisen aller Properties für die aktuelle Instanz
                            {
                                line_instanzen = reader.ReadLine();
                                if (line_instanzen.Contains("<instanz Index ="))
                                {
                                    continue; //nur Metadaten, wird übersprungen
                                }
                                if (line_instanzen.Contains("</instanz>"))
                                {
                                    break; //Ende der aktuellen Instanz
                                }

                                int posiStart_instanz_proptertyName = line_instanzen.IndexOf("\"") + 1;
                                int posiEnde_instanz_proptertyName = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyName);
                                string propertyName = line_instanzen.Substring(posiStart_instanz_proptertyName, posiEnde_instanz_proptertyName - posiStart_instanz_proptertyName);

                                int posiStart_instanz_proptertyTyp = line_instanzen.IndexOf("\"", posiEnde_instanz_proptertyName + 2) + 1;
                                int posiEnde_instanz_proptertyTyp = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyTyp);
                                //nicht benötigt => wird über die PropertyInfo besorgt
                                //string propertyTyp = line_instanzen.Substring(posiStart_instanz_proptertyTyp, posiEnde_instanz_proptertyTyp - posiStart_instanz_proptertyTyp); 

                                int posiStart_instanz_proptertyValue = line_instanzen.IndexOf("\"", posiEnde_instanz_proptertyTyp + 2) + 1;
                                int posiEnde_instanz_proptertyValue = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyValue);
                                string propertyValue_string = line_instanzen.Substring(posiStart_instanz_proptertyValue, posiEnde_instanz_proptertyValue - posiStart_instanz_proptertyValue);
                                PropertyInfo info = typ_klasse.GetProperties().FirstOrDefault(x => x.Name.Equals(propertyName));
                                try
                                {
                                    var converter = TypeDescriptor.GetConverter(info.PropertyType);
                                    var propertyValue = converter.ConvertFromString(propertyValue_string);

                                    info.SetValue(neue_instanz, Convert.ChangeType(propertyValue, info.PropertyType), null);
                                    erfolgreicheZeilen.Add(line_instanzen);
                                }
                                catch (Exception e)
                                {
                                    //Wert kann nicht geschrieben/geparst werden
                                    fehlerGefunden = true;
                                    fehlerhafteZeilen.Add(line_instanzen);
                                    fehlerMeldungen.Add(e.Message);
                                }
                            }

                            if (fehlerGefunden == false)
                            {
                                erfolgreiche_importeListe.Add(neue_instanz);
                                counter_importierteDatensaetze++;
                            }
                            else //fehlerGefunden == true
                            {
                                counter_fehlerhafterDatensaetze++;
                                var guidListe = typ_klasse.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.PropertyType == typeof(Guid));
                                string guidString = "";
                                if (guidListe.Count() == 1)
                                {
                                    //instanz_propertyStringListe.Add($"<instanz Index = \"{index}\" >");
                                    guidString = $"Guid: \"{guidListe.ElementAt(0).GetValue(neue_instanz, null)}\"";
                                }
                                else if (guidListe.Count() == 0)
                                {
                                    guidString = "Guid: \"keine Guid vorhanden\"";
                                }
                                else if (guidListe.Count() > 1)
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    stringBuilder.Append("mehrere Guids: \"");
                                    foreach (var guidProperty in guidListe)
                                    {
                                        stringBuilder.Append(guidProperty.GetValue(neue_instanz, null));
                                        stringBuilder.Append(", ");
                                    }
                                    stringBuilder = stringBuilder.Remove(stringBuilder.Length - 2, 2); //um das letzt überschüssige ", " zu entfernen
                                    stringBuilder.Append("\"");
                                    guidString = stringBuilder.ToString();
                                }

                                using (StreamWriter writer_fehler = new StreamWriter(filePath_Fehlerliste, true))
                                {
                                    writer_fehler.WriteLine($"Klasse: {typ_klasse} {guidString}, fehlerhafte Zeilen beim Import:");
                                    for (int i = fehlerMeldungen.Count() - 1; i >= 0; i--)
                                    {
                                        writer_fehler.WriteLine(fehlerhafteZeilen[i]);
                                        writer_fehler.WriteLine("Fehlermeldung:");
                                        writer_fehler.WriteLine(fehlerMeldungen[i]);
                                    }
                                    writer_fehler.WriteLine("--------------------");
                                }
                            }
                        }
                    }
                }
                var abfdgdgdgdc = erfolgreiche_importeListe;
            }

            string ausgabe = "";
            if (counter_importierteDatensaetze == 0)
            {
                ausgabe += "Es wurde kein Datensatz erfolgreich importiert";
            }
            else if (counter_importierteDatensaetze == 1)
            {
                ausgabe += "Es wurde 1 Datensatz erfolgreich importiert";
            }
            else
            {
                ausgabe += $"Es wurden {counter_importierteDatensaetze} Datensatz erfolgreich importiert";
            }

            if (counter_fehlerhafterDatensaetze == 1)
            {
                ausgabe += $"\r\n1 Datensatz konnte nicht importiert werden. Bitte überprüfen Sie das Fehlerprotokoll unter \"{filePath_Fehlerliste}\"";
            }
            else if (counter_fehlerhafterDatensaetze > 1)
            {
                ausgabe += $"\r\n{counter_fehlerhafterDatensaetze} Datensätze konnten nicht importiert werden. Bitte überprüfen Sie das Fehlerprotokoll unter \"{filePath_Fehlerliste}\"";
            }

            return ausgabe;
        }

        public static string Import_Static_Objekt(string fileName)
        {
            string ordnerName = StringHelper.Besorge_OrdnerPfad_Von_DateiPfad(fileName);
            string filePath_Fehlerliste = fileName.Insert(fileName.LastIndexOf("\\") + 1, "FehlerlisteImport_");
            Erstelle_OrdnerStruktur_wennNoetig(ordnerName);
            int counter_importierteDatensaetze = 0;
            int counter_fehlerhafterDatensaetze = 0;
            bool fehlerGefunden = false;
            string guidString = "";
            Type ermittelter_typ = null;
            object importiertes_static_object = new List<Object>();
            //List<Object> fehlerhafte_importeListe = new List<Object>();
            List<string> fehlerhafteZeilen = new List<string>();
            List<string> erfolgreicheZeilen = new List<string>();
            List<string> fehlerMeldungen = new List<string>();

            using (StreamReader reader = new StreamReader(fileName))
            {
                while (true)
                {
                    string line_klasse = reader.ReadLine();
                    if (line_klasse == null)
                    {
                        break;
                    }

                    if (line_klasse.Contains("<class:")) //Die ersten Zeilen sind nur Metadaten und werden daher übersprungen
                    {
                        //Bestimmung der Klasse durch Reflexion
                        int posiStart_klasse = line_klasse.IndexOf("\"") + 1;
                        string klassenName = line_klasse.Substring(posiStart_klasse, line_klasse.IndexOf(",") - posiStart_klasse);
                        AssemblyHelper.Struct_AssemblyQualifiedName importStruct = AssemblyHelper.Loese_AssemblyQualifiedName_auf(line_klasse);
                        ermittelter_typ = Assembly.Load(importStruct.Assembly_Name).GetType(importStruct.Klasse_FullName);
                        object static_object = ermittelter_typ;

                        while (true) //auslesen und zuweisen aller Properties für die aktuelle Instanz
                        {
                            string line_instanzen = reader.ReadLine();

                            if (string.IsNullOrEmpty(line_instanzen))
                            {
                                break;
                            }

                            if (line_instanzen.Contains("/class:"))
                            {
                                break;
                            }

                            if (line_instanzen.Contains("<instanz = "))
                            {
                                //auslesen der Guid
                                int posiStart_instanz_proptertyName2 = line_instanzen.IndexOf("\"") + 1;
                                int posiEnde_instanz_proptertyName2 = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyName2);
                                guidString = line_instanzen.Substring(posiStart_instanz_proptertyName2, posiEnde_instanz_proptertyName2 - posiStart_instanz_proptertyName2);
                            }
                            else if (line_instanzen.Contains("<instanz"))
                            {
                                continue; //nur Metadaten, wird übersprungen
                            }
                            if (line_instanzen.Contains("</instanz>"))
                            {
                                break; //Ende der aktuellen Instanz
                            }

                            int posiStart_instanz_proptertyName = line_instanzen.IndexOf("\"") + 1;
                            int posiEnde_instanz_proptertyName = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyName);
                            string propertyName = line_instanzen.Substring(posiStart_instanz_proptertyName, posiEnde_instanz_proptertyName - posiStart_instanz_proptertyName);

                            int posiStart_instanz_proptertyTyp = line_instanzen.IndexOf("\"", posiEnde_instanz_proptertyName + 2) + 1;
                            int posiEnde_instanz_proptertyTyp = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyTyp);

                            int posiStart_instanz_proptertyValue = line_instanzen.IndexOf("\"", posiEnde_instanz_proptertyTyp + 2) + 1;
                            int posiEnde_instanz_proptertyValue = line_instanzen.IndexOf("\"", posiStart_instanz_proptertyValue);
                            string propertyValue_string = line_instanzen.Substring(posiStart_instanz_proptertyValue, posiEnde_instanz_proptertyValue - posiStart_instanz_proptertyValue);

                            if (propertyValue_string.Contains("#&_1"))
                            {
                                //Beim Export wird ein Anführungszeichen in #&_1 umgewandelt, weil sonst das Auslesen nicht richtig funktioniert
                                propertyValue_string = propertyValue_string.Replace("#&_1", "\"");
                            }

                            PropertyInfo info = ermittelter_typ.GetProperties().FirstOrDefault(x => x.Name.Equals(propertyName));                        
                            try
                            {
                                var converter = TypeDescriptor.GetConverter(info.PropertyType);
                                var propertyValue = converter.ConvertFromString(propertyValue_string);

                                info.SetValue(static_object, Convert.ChangeType(propertyValue, info.PropertyType), null);
                                erfolgreicheZeilen.Add(line_instanzen);
                            }
                            catch (Exception e)
                            {
                                //Wert kann nicht geschrieben/geparst werden
                                fehlerGefunden = true;
                                fehlerhafteZeilen.Add(line_instanzen);
                                fehlerMeldungen.Add(e.Message);
                            }
                        }

                        if (fehlerGefunden == false)
                        {
                            importiertes_static_object = (static_object);
                            counter_importierteDatensaetze++;
                        }
                        else //fehlerGefunden == true
                        {
                            counter_fehlerhafterDatensaetze++;
                        }
                    }
                }
            }

            if (fehlerGefunden)
            {
                using (StreamWriter writer_fehler = new StreamWriter(filePath_Fehlerliste, true))
                {
                    if (string.IsNullOrEmpty(guidString))
                    {
                        writer_fehler.WriteLine($"Klasse: {ermittelter_typ}, fehlerhafte Zeilen beim Import:");
                    }
                    else
                    {
                        writer_fehler.WriteLine($"Klasse: {ermittelter_typ}, Guid: {guidString}, fehlerhafte Zeilen beim Import:");
                    }

                    for (int i = fehlerMeldungen.Count() - 1; i >= 0; i--)
                    {
                        writer_fehler.WriteLine(fehlerhafteZeilen[i]);
                        writer_fehler.WriteLine("Fehlermeldung:");
                        writer_fehler.WriteLine(fehlerMeldungen[i]);
                    }
                    writer_fehler.WriteLine("--------------------");
                }
            }

            string ausgabe = "";
            if (counter_fehlerhafterDatensaetze == 0)
            {
                if (counter_importierteDatensaetze == 0)
                {
                    ausgabe += "Es wurde kein Datensatz erfolgreich importiert";
                }
                else if (counter_importierteDatensaetze == 1)
                {
                    ausgabe += "Es wurde 1 Datensatz erfolgreich importiert";
                }
                else
                {
                    ausgabe += $"Es wurden {counter_importierteDatensaetze} Datensatz erfolgreich importiert";
                }
            }

            if (counter_fehlerhafterDatensaetze == 1)
            {
                ausgabe += $"\r\n1 Datensatz konnte nicht importiert werden. Bitte überprüfen Sie das Fehlerprotokoll unter \"{filePath_Fehlerliste}\"";
            }
            else if (counter_fehlerhafterDatensaetze > 1)
            {
                ausgabe += $"\r\n{counter_fehlerhafterDatensaetze} Datensätze konnten nicht importiert werden. Bitte überprüfen Sie das Fehlerprotokoll unter \"{filePath_Fehlerliste}\"";
            }

            object debug = importiertes_static_object;
            return ausgabe;
        }

        public static (bool fehler, string filepath) Besorge_letztenExport(bool abweichenderSpeicherort, string filepath)
        {
            string zuUeberpruefen = abweichenderSpeicherort ? filepath : Default_Speicherort;
            if (!Directory.Exists(zuUeberpruefen))
            {
                return (true, "Der Ordnerpfad wurde nicht gefunden!");
            }
            else
            {
                var list = Directory.GetFiles(zuUeberpruefen);
                if (list.Length == 0)
                {
                    return (true, "");
                }
                Dictionary<string, DateTime> uhrzeiten_Liste = new Dictionary<string, DateTime>();
                foreach (var gefundeneFiles in list)
                {
                    using (StreamReader reader = new StreamReader(gefundeneFiles))
                    {
                        string line = reader.ReadLine();
                        var uhrzeit = Besorge_Datum_von_Exportdatei(line);
                        if (uhrzeit == DateTime.MinValue)
                        {
                            return (true, "");
                        }
                        uhrzeiten_Liste.Add(gefundeneFiles, uhrzeit);
                    }
                }
                var test = uhrzeiten_Liste.Max(x => x.Value);
                var test2 = uhrzeiten_Liste.ToList();
                var test3 = test2.FirstOrDefault(x => x.Value == test);
                return (false, test3.Key);
            }
        }

        public static DateTime Besorge_Datum_von_Exportdatei(string importString)
        {
            if (string.IsNullOrEmpty(importString))
            {
                return DateTime.MinValue;
            }

            int posi = 0;
            posi = importString.IndexOf('\"') + 1;
            posi = importString.IndexOf('\"', posi) + 1;
            posi = importString.IndexOf('\"', posi) + 1;
            int posiEnde = importString.LastIndexOf('\"');

            if (posiEnde == -1)
            {
                return DateTime.Parse(importString.Substring(posi, posiEnde - posi));
            }

            return DateTime.Parse(importString.Substring(posi, posiEnde - posi));
        }

        //public static bool ErstelleExport_Liste(string fileName, List<Object> exportObjectListe, out string hinweis)
        //{
        //    if (exportObjectListe.Count > 0)
        //    {
        //        foreach (var exportObject in exportObjectListe)
        //        {
        //            if (!exportObject.GetType().IsClass)
        //            {
        //                hinweis = "Der Export wurde abgebrochen, da das zu exportierende Objekt keine Klasse ist";
        //                return false;
        //            }
        //            if (exportObject.GetType().GetProperties(BindingFlags.Public).Length == 0)
        //            {
        //                hinweis = "Der Export wurde abgebrochen, da das zu exportierende Objekt keine öffentlichen Eigenschaften hat";
        //                return false;
        //            }

        //            hinweis = "alles gut";
        //            return true;

        //            #region
        //            //        int counter_erfolgreich = 0;
        //            //        int counter_nichtErfolgreich = 0;
        //            //        string verzeichnis = $"{fileName.Substring(0, fileName.LastIndexOf("\\") + 1)}";
        //            //        string fehlerListe_Dateiname = "Fehlerliste.txt";

        //            //        using (StreamWriter writer = new StreamWriter(fileName))
        //            //        {
        //            //            //Kopf:
        //            //            string projektName = Assembly.GetExecutingAssembly().GetName().Name;
        //            //            writer.WriteLine($"Export durch \"{projektName}\" am {DateTime.Now}\r\n");
        //            //            writer.WriteLine($"<class: \"{listBox_ausgabe.Items.GetType()}\">");


        //            //            foreach (var item in listBox_eingabe.Items)
        //            //            {
        //            //                if (((string)item).Contains(">") || ((string)item).Contains("<"))
        //            //                {
        //            //                    counter_nichtErfolgreich++;
        //            //                    using (StreamWriter writer_fehlerhaft = new StreamWriter(verzeichnis + fehlerListe_Dateiname))
        //            //                    {
        //            //                        writer_fehlerhaft.WriteLine($"Objekt: \"{item}\", Begründung: Verbotenes Zeichen entdeckt: \"<\" oder \">\"");
        //            //                    }
        //            //                }
        //            //                else
        //            //                {
        //            //                    counter_erfolgreich++;
        //            //                    DummyKlasse dummy = new DummyKlasse();
        //            //                    dummy.Beschreibung = item.ToString();
        //            //                    writer.WriteLine($"<\"key\": \"{item}\">");
        //            //                }
        //            //            }
        //            //            writer.WriteLine($"</class: \"{listBox_ausgabe.Items.GetType()}\">");
        //            //        }

        //            //        if (counter_nichtErfolgreich == 0 && counter_erfolgreich > 0)
        //            //        {
        //            //            textBox_ausgabe.Text += $"Es wurden erfolgreich {counter_erfolgreich} Datensätze exportiert.\r\n";
        //            //        }
        //            //        else if (counter_nichtErfolgreich > 0 && counter_erfolgreich > 0)
        //            //        {
        //            //            textBox_ausgabe.Text += $"Es wurden erfolgreich {counter_erfolgreich} Datensätze exportiert.\r\n";
        //            //            textBox_ausgabe.Text += $"{counter_nichtErfolgreich} Datensätze konnten nicht exportiert werden." +
        //            //                $" Für weitere Infos, überprüfen Sie bitte das Fehlerprotokoll \"{fehlerListe_Dateiname}\" in dem Verzeichnis \"{verzeichnis}\".\r\n";
        //            //        }
        //            //        else if (counter_nichtErfolgreich > 0 && counter_erfolgreich == 0)
        //            //        {
        //            //            textBox_ausgabe.Text += $"{counter_nichtErfolgreich} Datensätze konnten nicht exportiert werden." +
        //            //$" Für weitere Infos, überprüfen Sie bitte das Fehlerprotokoll \"{fehlerListe_Dateiname}\" in dem Verzeichnis \"{verzeichnis}\".\r\n";
        //            //        }
        //            //        else if (counter_nichtErfolgreich == 0 && counter_erfolgreich == 0)
        //            //        {
        //            //            textBox_ausgabe.Text += $"Es wurden keine Datensätze importiert.\r\n";
        //            //        }
        //            #endregion
        //        }
        //    }

        //    hinweis = "Die Liste der zu exportierenden Objekte hat keinen Inhalt. Deswegen wurde nichts exportiert.";
        //    return false;
        //}

        public static string ErstelleDateiNamen(string nameofObject)
        {
            return $"{nameofObject}_{DateTime.Now}".Replace(":", "_").Replace(".", "_").Replace(" ", "_") + ".txt";
        }

        public static DateTime BesorgeDateTimeVonDateiNamen(string dateiName)
        {
            var lines = dateiName.Split('_');
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                lines[i] = lines[i].Replace(".txt", "");
            }

            try
            {
                return new DateTime(int.Parse(lines[3]), int.Parse(lines[2]), int.Parse(lines[1]), int.Parse(lines[4]), int.Parse(lines[5]), int.Parse(lines[6]));
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
    }
}
