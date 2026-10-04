
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    public class Canvas : Panel
    {
        public Canvas() { }

        public Canvas(int width, int height)
        {
            Name = "Canvas";
            Width = width;
            Height = height;
            Location = new Point(0, Helper.Mainform.Actionbar.Height);
            Resize_Update_Properties();
            this.Resize += Canvas_Resize;
            this.Paint += Canvas_Paint;
        }


        #region Properties
        private Block Maze_Start { get { return new Block(1, 1); } }
        private Block Maze_Ende { get { return new Block(Maze_Rohdaten.Length - 2, Maze_Rohdaten[0].Length - 2); } }
        public bool Maze_zeichnen_erlaubt { get; set; } = true;
        public Random Rnd { get; set; } = new Random();
        public int ZuRendernderErstellfortschritt { get; set; }
        public bool ReRenderDurchErstellvorgang { get; set; }
        public Button RenderCaller { get; set; }
        public Block[] BesuchteFelder_links { get; set; }
        public int BesuchteFelder_links_counter { get; set; }
        public Block[] BesuchteFelder_rechts { get; set; }
        public int BesuchteFelder_rechts_counter { get; set; }

        private int anzahlDerBloecke_X = 0;
        public int AnzahlDerBloecke_X
        {
            get
            {
                return anzahlDerBloecke_X;
            }
            set
            {
                anzahlDerBloecke_X = value;
                Form form = Helper.Aktive_Forms.FirstOrDefault(x => x.GetType() == typeof(Form_Performence));
                if (form != null)
                {
                    Form_Performence form_Performence = form as Form_Performence;
                    form_Performence.Bloecke_Richtung_X = anzahlDerBloecke_X;
                }
            }
        }

        private int anzahlDerBloecke_Y = 0;
        public int AnzahlDerBloecke_Y
        {
            get
            {
                return anzahlDerBloecke_Y;
            }
            set
            {
                anzahlDerBloecke_Y = value;
                Form form = Helper.Aktive_Forms.FirstOrDefault(x => x.GetType() == typeof(Form_Performence));
                if (form != null)
                {
                    Form_Performence form_Performence = form as Form_Performence;
                    form_Performence.Bloecke_Richtung_Y = anzahlDerBloecke_Y;
                }
            }
        }

        public bool[][] Maze_Rohdaten { get; set; }
        public Block[] Maze_ErstellReihenfolge { get; set; }
        public bool ZeigeHinweis { get; set; } = false;
        public Point Startpunkt { get; set; } = new Point(App_Einstellungen.Margin, App_Einstellungen.Margin);
        public bool IstGezeichnet { get; set; } = false;
        public Stopwatch _Stopwatch { get; set; }
        public int BereitsBelegteBloecke = 0;
        public int BereitsBelegteBloecke_EndeRahmen = 0;

        #endregion Properties

        #region Events
        private void Canvas_Resize(object sender, EventArgs e)
        {
            Resize_Update_Properties();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            if (Maze_zeichnen_erlaubt && Maze_Rohdaten != null)
            {
                #region erstellen der Rectangles
                Form_Performence form = (Form_Performence)Helper.Aktive_Forms.FirstOrDefault(x => x.GetType() == typeof(Form_Performence));
                bool[][] maze_rohdaten = Maze_Rohdaten;
                Rectangle[][] maze_rectangles = new Rectangle[maze_rohdaten.Length][];
                int blockgroesse = App_Einstellungen.Blockgroesse;
                bool schraenkeRendernEin = false;

                if (form != null)
                {
                    if (ReRenderDurchErstellvorgang)
                    {
                        form.Anzahl_erstellterBloecke = BereitsBelegteBloecke;
                        form.Trackbar_Erstellreihenfolge.Enabled = true;
                        form.Trackbar_Erstellreihenfolge.LargeChange = Convert.ToInt32((BereitsBelegteBloecke + BesuchteFelder_links_counter) / 20);
                        form.Trackbar_Erstellreihenfolge.SmallChange = Convert.ToInt32((BereitsBelegteBloecke + BesuchteFelder_links_counter) / 50);
                        form.Trackbar_Erstellreihenfolge.Minimum = 0;
                        form.Trackbar_Erstellreihenfolge.Maximum = BereitsBelegteBloecke + BesuchteFelder_links_counter;
                        form.Trackbar_Erstellreihenfolge.Value = BereitsBelegteBloecke + BesuchteFelder_links_counter;
                        form.Bloecke_Richtung_X = AnzahlDerBloecke_X;
                        form.Bloecke_Richtung_Y = anzahlDerBloecke_Y;
                    }
                    if ((BereitsBelegteBloecke + BesuchteFelder_links_counter ) > form.Trackbar_Erstellreihenfolge.Value && form.Trackbar_Erstellreihenfolge.Enabled)
                    {
                        schraenkeRendernEin = true;
                        Rectangle[] maze_rectangles_ligth = new Rectangle[form.Trackbar_Erstellreihenfolge.Value];
                        Brush brush_black = new SolidBrush(Color.Black);

                        int anzahl;
                        if (BereitsBelegteBloecke < form.Trackbar_Erstellreihenfolge.Value)
                        {
                            anzahl = BereitsBelegteBloecke;
                        }
                        else
                        {
                            anzahl = form.Trackbar_Erstellreihenfolge.Value;
                        }

                        for (int i = 0; i < anzahl; i++)
                        {
                            Rectangle rec = new Rectangle(Besorge_Punkt(Maze_ErstellReihenfolge[i].pos_y, Maze_ErstellReihenfolge[i].pos_x), new Size(blockgroesse, blockgroesse));
                            e.Graphics.FillRectangle(brush_black, rec);
                        }

                        int anzahl_blaueWege = form.Trackbar_Erstellreihenfolge.Value - BereitsBelegteBloecke;
                        if (anzahl_blaueWege < 0)
                        {
                            anzahl_blaueWege = 0;
                        }

                        //rendern des linken Pfades
                        Rectangle[] rectangles_besucht = new Rectangle[BesuchteFelder_links_counter];
                        Block[] besucht = BesuchteFelder_links;
                        Brush brush_blue = new SolidBrush(Color.LightBlue);
                        for (int k = 0; k < anzahl_blaueWege; k++)
                        {
                            Rectangle rec = new Rectangle(Besorge_Punkt(besucht[k].pos_y, besucht[k].pos_x), new Size(blockgroesse, blockgroesse));
                            e.Graphics.FillRectangle(brush_blue, rec);
                        }
                        brush_blue.Dispose();
                    }
                }

                for (int i = 0; i < maze_rohdaten.Length; i++)
                {
                    maze_rectangles[i] = new Rectangle[maze_rohdaten[0].Length];
                }

                for (int j = 0; j < maze_rectangles.Length; j++)
                {
                    for (int k = 0; k < maze_rectangles[j].Length; k++)
                    {
                        if (maze_rohdaten[j][k] == true)
                        {
                            maze_rectangles[j][k] = new Rectangle(Besorge_Punkt(j, k), new Size(blockgroesse, blockgroesse));
                        }
                    }
                }

                if (schraenkeRendernEin == false) // default vorgehen
                {
                    Brush brush_black = new SolidBrush(Color.Black);

                    for (int i = 0; i < maze_rectangles.Length; i++)
                    {
                        for (int j = 0; j < maze_rectangles[i].Length; j++)
                        {
                            //e.Graphics.DrawRectangle(pen, _Canvas.Maze_Rectangles[i][j]);
                            e.Graphics.FillRectangle(brush_black, maze_rectangles[i][j]);
                        }
                    }
                    brush_black.Dispose();

                    //rendern des linken Pfades
                    Rectangle[] rectangles_besucht = new Rectangle[BesuchteFelder_links_counter];
                    Block[] besucht = BesuchteFelder_links;
                    Brush brush_blue = new SolidBrush(Color.LightBlue);
                    for (int k = 0; k < BesuchteFelder_links_counter; k++)
                    {
                        Rectangle rec = new Rectangle(Besorge_Punkt(besucht[k].pos_y, besucht[k].pos_x), new Size(blockgroesse, blockgroesse));
                        e.Graphics.FillRectangle(brush_blue, rec);
                    }
                    brush_blue.Dispose();
                }
                #endregion

                Brush brush_green = new SolidBrush(Color.LightGreen);
                //brush. = Color.LightGreen;
                e.Graphics.FillRectangle(brush_green, maze_rectangles[0][1]); //Eingang
                brush_green.Dispose();

                Brush brush_red = new SolidBrush(Color.OrangeRed);
                int lengthY = maze_rectangles.Length;
                int lengthX = maze_rectangles[lengthY - 1].Length;
                e.Graphics.FillRectangle(brush_red, maze_rectangles[lengthY - 1][lengthX - 2]); //Ausgang
                brush_red.Dispose();

                RenderCaller.Enabled = true;
                _Stopwatch.Stop();
                if (ReRenderDurchErstellvorgang)
                {
                    ReRenderDurchErstellvorgang = false;
                    double sekunden = Math.Round(_Stopwatch.ElapsedMilliseconds / 1000.0, 3);
                    if (ZeigeHinweis)
                    {
                        MessageBox.Show($"Das Maze wurde in {sekunden} s gerendert.");
                    }

                    if (form != null)
                    {
                        form.Dauer_der_Berechnung = sekunden;
                    }
                }

                IstGezeichnet = true;
            }
        }
        #endregion Events

        #region Methoden
        public bool Ueberpruefe_Posi_ob_frei(int x, int y)
        {
            if (x > 0 && y > 0)
            {
                if (Maze_Rohdaten[y][x] == false)
                {
                    return true; //nächster Block darf gesetzt werden
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private void Erstelle_Maze_Inhalt()
        {
            if (BereitsBelegteBloecke > 0 && Maze_Rohdaten != null)
            {
                bool[][] maze = Maze_Rohdaten;
                int laenge_y = Maze_Rohdaten.Length;
                int laenge_x = Maze_Rohdaten[0].Length;
                int canvas_bereitsErstelleBloecke = BereitsBelegteBloecke;
                int rahmen_Gewichtung = App_Einstellungen.Gewichtung_Rahmen;
                List<int> verfuegbareBloecke = new List<int>();
                for (int i = 0; i < canvas_bereitsErstelleBloecke; i++)
                {
                    verfuegbareBloecke.Add(i);
                }
                #region alt
                // int anzahlZuErstellen = (int)numericUpDown_bloecke.Value;
                // bool ueberspringeZufaelligeAuswahl = false;
                // Block[] sichAusdehnendeBloecke = null;
                //int[] sichAusdehnendeBloecke_index = null;
                //if (anzahlZuErstellen >= canvas_bereitsErstelleBloecke)
                //{
                //    anzahlZuErstellen = canvas_bereitsErstelleBloecke;
                //    sichAusdehnendeBloecke = new Block[anzahlZuErstellen];
                //    //sichAusdehnendeBloecke_index = new int[anzahlZuErstellen];
                //    ueberspringeZufaelligeAuswahl = true;

                //    for (int i = 0; i <= anzahlZuErstellen; i++)
                //    {
                //        sichAusdehnendeBloecke[i] = canvas.Maze_ErstellReihenfolge[i];
                //    }
                //}

                //if (ueberspringeZufaelligeAuswahl == false)
                //{
                //    int counter = 0;
                //    sichAusdehnendeBloecke = new Block[anzahlZuErstellen];
                //    //sichAusdehnendeBloecke_index = new int[anzahlZuErstellen];
                //    List<int> schonAusgewaehlteBloecke = new List<int>();

                //    while (counter < anzahlZuErstellen) //Es werden zufällige Blöcke von den bereits erstellten ausgewählt
                //    {
                //        int rnd = Rnd.Next(0, canvas_bereitsErstelleBloecke);
                //        if (schonAusgewaehlteBloecke.Contains(rnd) == false)
                //        {
                //            schonAusgewaehlteBloecke.Add(rnd);
                //            sichAusdehnendeBloecke[counter] = canvas.Maze_ErstellReihenfolge[rnd];
                //            //sichAusdehnendeBloecke_index[counter] = rnd;
                //            counter++;
                //        }
                //    }
                //}
                #endregion

                while (verfuegbareBloecke.Count > 0)
                {
                    int rnd = Rnd.Next(0, 101);
                    int rnd_auswahl = 0;

                    if (rnd > rahmen_Gewichtung) //Benutzung der zuletzt erstellten Blöcke
                    {
                        int suchraum = Convert.ToInt32(verfuegbareBloecke.Count * 0.95);
                        if (suchraum == verfuegbareBloecke.Count)
                        {
                            suchraum = 0;
                        }
                        rnd_auswahl = Rnd.Next(suchraum, verfuegbareBloecke.Count);
                        //if (verfuegbareBloecke.Count == 1)
                        //{
                        //    rnd_auswahl = 0;
                        //}
                    }
                    else
                    {
                        rnd_auswahl = Rnd.Next(0, verfuegbareBloecke.Count);
                    }

                    int index = verfuegbareBloecke[rnd_auswahl];
                    Block block = Maze_ErstellReihenfolge[index];
                    if (block == null)
                    {
                        var debug = index;
                        var debug2 = Maze_ErstellReihenfolge;
                    }
                    bool erfolgreich = ErstelleNeuenBlock_wennMoeglich(block, ref verfuegbareBloecke);
                    if (erfolgreich == false)
                    {
                        verfuegbareBloecke.RemoveAt(rnd_auswahl);
                    }
                }

                LoeseMaze_links(10);
                if (Maze_zeichnen_erlaubt)
                {
                    ReRenderDurchErstellvorgang = true;
                    this.Refresh();
                }
                #region alt
                //(int vorne, int rechts) Erstelle_Richtung(int startrichtung)
                //{
                //    // -1 : 0 Norden
                //    // 0 : 1 Osten
                //    // 1 : 0 Süden
                //    // 0 : -1 Westen
                //    if (startrichtung == 0)                        // -1 : 0 Norden
                //        {
                //        return (-1, 0);
                //    }
                //    else if (startrichtung == 1)                        // 0 : 1 Osten
                //        {
                //        return (0, 1);
                //    }
                //    else if(startrichtung == 2)                        // 1 : 0 Süden
                //        {
                //        return (1, 0);
                //    }
                //    else
                //    {
                //        return (0, -1);                        // 0 : -1 Westen
                //        }
                //}
                #endregion
            }
        }

        //private void Erstelle_Ast(Block block, int richtungPraeferenz)

        /// <summary>
        /// true => Weg ist frei, false => weg ist versperrt
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private bool Ueberpruefe_existiertPunkt_runner(int y, int x)
        {
            if (Maze_Rohdaten[y][x] == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Ueberpruefe_existiertPunkt(int y, int x, int laenge_y, int laenge_x)
        {
            if (y >= 0 && x >= 0 && y < laenge_y && x < laenge_x)
            {
                if (Maze_Rohdaten[y][x] == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool ErstelleNeuenBlock_wennMoeglich(Block block,  ref List<int> verfuegbareBloecke)
        {
            int laenge_y = Maze_Rohdaten.Length;
            int laenge_x = Maze_Rohdaten[0].Length;
            int richtung_index = Rnd.Next(0, 4); // Startrichtung
            int[] richtung_reihenfolge = new int[4];
            richtung_reihenfolge[0] = richtung_index;
            for (int i = 1; i < 4; i++) //Erstellung der Reihenfolge für das überprüfen der Richtungen
            {
                richtung_index++;
                if (richtung_index > 3)
                {
                    richtung_index = 0;
                }

                richtung_reihenfolge[i] = richtung_index;
            }

            bool erfolgreich = false;
            for (int i = 0; i < 4; i++) //für alle vier Richtungen
            {
                if (erfolgreich)
                {
                    break;
                }

                #region alt
                //if (Ueberpruefe_existiertPunkt(block.pos_y - (1 * richtung.vorne), block.pos_x))
                //{ //Blickrichtung vom Block eins nach vorne

                //}
                //else if (Ueberpruefe_existiertPunkt(block.pos_y - (1 * richtung.vorne), block.pos_x - (1 * richtung.rechts)))
                //{ //Blickrichtung vom Block eins nach vorne, eins nach rechts

                //}
                //else if (Ueberpruefe_existiertPunkt(block.pos_y - 1 * richtung.vorne, block.pos_x + 1 * richtung.rechts))
                //{//Blickrichtung vom Block eins nach vorne, eins nach links

                //}
                //else if (Ueberpruefe_existiertPunkt(block.pos_y - 2 * richtung.vorne, block.pos_x))
                //{ //Blickrichtung vom Block zwei nach vorne

                //}
                //else if (Ueberpruefe_existiertPunkt(block.pos_y - 2 * richtung.vorne, block.pos_x - 1 * richtung.rechts))
                //{ //Blickrichtung vom Block zwei nach vorne, eins nach links

                //}
                //else if (Ueberpruefe_existiertPunkt(block.pos_y - 2 * richtung.vorne, block.pos_x + 1 * richtung.rechts))
                //{//Blickrichtung vom Block zwei nach vorne, eins nach rechts

                //}
                #endregion

                (int y, int x)[] ueberpruefpunkte_Richtung = new (int y, int x)[6];
                if (richtung_reihenfolge[i] == 0)
                {
                    ueberpruefpunkte_Richtung[0] = (-1, 0);
                    ueberpruefpunkte_Richtung[1] = (-1, 1);
                    ueberpruefpunkte_Richtung[2] = (-1, -1);
                    ueberpruefpunkte_Richtung[3] = (-2, 0);
                    ueberpruefpunkte_Richtung[4] = (-2, 1);
                    ueberpruefpunkte_Richtung[5] = (-2, -1);
                }
                else if (richtung_reihenfolge[i] == 1)
                {
                    ueberpruefpunkte_Richtung[0] = (0, 1);
                    ueberpruefpunkte_Richtung[1] = (1, 1);
                    ueberpruefpunkte_Richtung[2] = (-1, 1);
                    ueberpruefpunkte_Richtung[3] = (0, 2);
                    ueberpruefpunkte_Richtung[4] = (1, 2);
                    ueberpruefpunkte_Richtung[5] = (-1, 2);
                }
                else if (richtung_reihenfolge[i] == 2)
                {
                    ueberpruefpunkte_Richtung[0] = (1, 0);
                    ueberpruefpunkte_Richtung[1] = (1, -1);
                    ueberpruefpunkte_Richtung[2] = (1, 1);
                    ueberpruefpunkte_Richtung[3] = (2, 0);
                    ueberpruefpunkte_Richtung[4] = (2, -1);
                    ueberpruefpunkte_Richtung[5] = (2, 1);
                }
                else
                {
                    ueberpruefpunkte_Richtung[0] = (0, -1);
                    ueberpruefpunkte_Richtung[1] = (-1, -1);
                    ueberpruefpunkte_Richtung[2] = (1, -1);
                    ueberpruefpunkte_Richtung[3] = (0, -2);
                    ueberpruefpunkte_Richtung[4] = (-1, -2);
                    ueberpruefpunkte_Richtung[5] = (1, -2);
                }

                bool blockErstellen_nichtmoeglich = false;
                for (int j = 0; j < ueberpruefpunkte_Richtung.Length; j++) //Überprüfung der 6 Blöcke vor dem aktuellen Punkt
                {
                    int y = ueberpruefpunkte_Richtung[j].y;
                    int x = ueberpruefpunkte_Richtung[j].x;
                    int aktuelleRichtung = richtung_reihenfolge[i];

                    int blockY = block.pos_y;
                    int blockX = block.pos_x;

                    if (Ueberpruefe_existiertPunkt(block.pos_y + y, block.pos_x + x, laenge_y, laenge_x) == false)
                    {
                        blockErstellen_nichtmoeglich = true;
                        break;
                    }
                }

                if (blockErstellen_nichtmoeglich == false)
                {
                    int verfuegbareStelle_y = block.pos_y + ueberpruefpunkte_Richtung[0].y;
                    int verfuegbareStelle_x = block.pos_x + ueberpruefpunkte_Richtung[0].x;
                    Block neuerBlock = new Block(verfuegbareStelle_y, verfuegbareStelle_x);

                    Maze_Rohdaten[verfuegbareStelle_y][verfuegbareStelle_x] = true;
                    Maze_ErstellReihenfolge[BereitsBelegteBloecke] = neuerBlock;
                    verfuegbareBloecke.Add(BereitsBelegteBloecke);
                    BereitsBelegteBloecke++;

                    return true;
                }
            }

            return false; //keine gültige Richtung gefunden
        }

        private void LoeseMaze_links(int schritte)
        {
            #region zurücksetzten der Werte
            int anzahl = AnzahlDerBloecke_X * AnzahlDerBloecke_Y;
            BesuchteFelder_links = new Block[anzahl];
            BesuchteFelder_links_counter = 0;
            #endregion

            Block[] besuchteFelder_links = new Block[anzahl];
            Block runner_links = new Block(1, 1);
            Block letztePosition = runner_links;
            Block neuerBlock = null;
            besuchteFelder_links[0] = new Block(1, 1);
            int besuchteFelder_links_counter = 1;
            Block ende = Maze_Ende;
            int richtung = 1;
            int letzteWand_Richtung_absolut = 0;
            //(bool leer, bool letztePosi)[] umgebung = new (bool leer, bool letztePosi)[4];
            int laenge_y = AnzahlDerBloecke_Y;
            int laenge_x = AnzahlDerBloecke_X;

            //int counter = 0;
            //while (runner_links.pos_y != ende.pos_y && runner_links.pos_x != ende.pos_x)
            while(true)
            {
                if (runner_links.pos_y == ende.pos_y && runner_links.pos_x == ende.pos_x)
                {
                    break;
                }

                #region alt
                //Überprüfen der Umgebung der aktuellen Position
                //letztePosition = runner_links;

                //    int[] ueberpruefungsReihenfolge = new int[4];
                //    ueberpruefungsReihenfolge[0] = letzteWand_Richtung_absolut;
                //    for (int i = 1; i < 4; i++)
                //    {
                //        int dummy = letzteWand_Richtung_absolut + i;
                //        if (dummy > 3)
                //        {
                //            dummy = dummy - 4;
                //        }
                //        ueberpruefungsReihenfolge[i] = dummy;
                //    }

                //    bool erfolgreich = false;
                //    for (int i = 0; i < 4; i++)
                //    {
                //        int ueberpruefung_richtung = ueberpruefungsReihenfolge[i];
                //        int richtung_puffer = richtung;
                //        (int y, int x)[] ueberpruefpunkte_Richtung = new (int y, int x)[4];
                //        if (ueberpruefung_richtung == 0) //Blickrichtung nach Norden
                //        {
                //            ueberpruefpunkte_Richtung[0] = (-1, 0); //vorne
                //            ueberpruefpunkte_Richtung[1] = (0, 1); //rechts
                //            ueberpruefpunkte_Richtung[2] = (1, 0); //hinten
                //            ueberpruefpunkte_Richtung[3] = (0, -1); //links
                //        }
                //        else if (ueberpruefung_richtung == 1) //Blickrichtung nach Osten
                //        {
                //            ueberpruefpunkte_Richtung[0] = (0, 1); //vorne
                //            ueberpruefpunkte_Richtung[1] = (1, 0); //rechts
                //            ueberpruefpunkte_Richtung[2] = (0, -1); //hinten
                //            ueberpruefpunkte_Richtung[3] = (-1, 0); //links
                //        }
                //        else if (ueberpruefung_richtung == 2)//Blickrichtung nach Süden
                //        {
                //            ueberpruefpunkte_Richtung[0] = (1, 0); //vorne
                //            ueberpruefpunkte_Richtung[1] = (0, -1); //rechts
                //            ueberpruefpunkte_Richtung[2] = (-1, 0); //hinten
                //            ueberpruefpunkte_Richtung[3] = (0, 1); //links
                //        }
                //        else //Blickrichtung nach Westen
                //        {
                //            ueberpruefpunkte_Richtung[0] = (0, -1); //vorne
                //            ueberpruefpunkte_Richtung[1] = (-1, 0); //rechts
                //            ueberpruefpunkte_Richtung[2] = (0, 1); //hinten
                //            ueberpruefpunkte_Richtung[3] = (1, 0); //linkss
                //        }

                //        for (int j = 0; j < 4; j++)
                //        {
                //            #region
                //            //Debug
                //            //if (zuUeberpruefenderBlock.pos_y != letztePosition.pos_y || zuUeberpruefenderBlock.pos_x != letztePosition.pos_x)
                //            //{


                //            //    }
                //            //else
                //            //  {
                //            //    umgebung[j] = (false, false);


                //            //}
                //            #endregion
                //            Block zuUeberpruefenderBlock = new Block(runner_links.pos_y + ueberpruefpunkte_Richtung[j].y, runner_links.pos_x + ueberpruefpunkte_Richtung[j].x);
                //            umgebung[j] = (Ueberpruefe_existiertPunkt_runner(runner_links.pos_y + ueberpruefpunkte_Richtung[j].y, runner_links.pos_x + ueberpruefpunkte_Richtung[j].x, laenge_y, laenge_x, letztePosition), false);

                //        }

                //        #region Überprüfung vorne, hinten, rechts, links
                //        if (umgebung[0].leer == true) // nach vorne in Blickrichtung
                //        {
                //            neuerBlock = new Block(runner_links.pos_y + ueberpruefpunkte_Richtung[0].y, runner_links.pos_x + ueberpruefpunkte_Richtung[0].x);
                //            richtung = ueberpruefung_richtung;
                //            erfolgreich = true;
                //            break;
                //        }
                //        else if(umgebung[0].leer == false && umgebung[0].letztePosi == false)
                //        {
                //            Block zuUeberpruefenderBlock = Bestimme_FehlendeWand(letzteWand_Richtung_absolut, richtung_puffer);
                //            if (Ueberpruefe_existiertPunkt(zuUeberpruefenderBlock.pos_y, zuUeberpruefenderBlock.pos_x, laenge_y, laenge_x) == true)
                //            {
                //                neuerBlock = zuUeberpruefenderBlock;
                //                richtung = letzteWand_Richtung_absolut;
                //                erfolgreich = true;
                //                break;
                //            }

                //            letzteWand_Richtung_absolut = ueberpruefung_richtung;
                //        }

                //        richtung = ueberpruefung_richtung + 1;
                //        if (richtung > 3)
                //        {
                //            richtung = richtung - 4;
                //        }
                //        if (umgebung[1].leer == true)
                //        {
                //            neuerBlock = new Block(runner_links.pos_y + ueberpruefpunkte_Richtung[1].y, runner_links.pos_x + ueberpruefpunkte_Richtung[1].x);
                //            erfolgreich = true;
                //            break;
                //        }
                //        else if(umgebung[1].leer == false && umgebung[1].letztePosi == false)
                //        {
                //            letzteWand_Richtung_absolut = richtung;
                //        }

                //        richtung = ueberpruefung_richtung + 2;
                //        if (richtung > 3)
                //        {
                //            richtung = richtung - 4;
                //        }
                //        if (umgebung[2].leer == true)
                //        {
                //            neuerBlock = new Block(runner_links.pos_y + ueberpruefpunkte_Richtung[2].y, runner_links.pos_x + ueberpruefpunkte_Richtung[2].x);
                //            richtung = ueberpruefung_richtung + 2;
                //            if (richtung > 3)
                //            {
                //                richtung = richtung - 4;
                //            }
                //            erfolgreich = true;
                //            break;
                //        }
                //        else if (umgebung[2].leer == false && umgebung[2].letztePosi == false)
                //        {
                //            letzteWand_Richtung_absolut = richtung;
                //        }

                //        richtung = ueberpruefung_richtung + 3;
                //        if (richtung > 3)
                //        {
                //            richtung = richtung - 4;
                //        }
                //        if (umgebung[3].leer == true)
                //        {
                //            neuerBlock = new Block(runner_links.pos_y + ueberpruefpunkte_Richtung[3].y, runner_links.pos_x + ueberpruefpunkte_Richtung[3].x);
                //            richtung = ueberpruefung_richtung + 3;
                //            if (richtung > 3)
                //            {
                //                richtung = richtung - 4;
                //            }
                //            erfolgreich = true;
                //            break;
                //        }
                //        else if (umgebung[3].leer == false && umgebung[3].letztePosi == false)
                //        {
                //            letzteWand_Richtung_absolut = richtung;
                //        }
                //        #endregion Überprüfung vorne, hinten, rechts, links
                //    }
                //    #region Überprüfung obenrechts, untenlinks, untenrechts, obenlinks
                //    //ausnahme: ende eines Astes erreicht => Überprüfung, wo es weiter geht
                //    //if nichterfolgreich
                //    if (erfolgreich == false)
                //    {
                //        for (int i = 0; i < 4; i++)
                //        {
                //            //int ueberpruefung_richtung = ueberpruefungsReihenfolge[i];
                //            //(int y, int x)[] ueberpruefpunkte_Richtung_ast = new (int y, int x)[4];
                //            //if (ueberpruefung_richtung == 0) //Blickrichtung nach Norden
                //            //{
                //            //    ueberpruefpunkte_Richtung_ast[0] = (-1, 1); //vorne rechts
                //            //    ueberpruefpunkte_Richtung_ast[1] = (1, 1); //hinten rechts
                //            //    ueberpruefpunkte_Richtung_ast[2] = (1, -1); //hinten links
                //            //    ueberpruefpunkte_Richtung_ast[3] = (0, -1); //vorne links
                //            //}
                //            //else if (richtung == 1) //Blickrichtung nach Osten
                //            //{
                //            //    ueberpruefpunkte_Richtung_ast[0] = (0, 1); //vorne
                //            //    ueberpruefpunkte_Richtung_ast[1] = (1, 0); //rechts
                //            //    ueberpruefpunkte_Richtung_ast[2] = (0, -1); //hinten
                //            //    ueberpruefpunkte_Richtung_ast[3] = (-1, 0); //links
                //            //}
                //            //else if (richtung == 2)//Blickrichtung nach Süden
                //            //{
                //            //    ueberpruefpunkte_Richtung_ast[0] = (-1, 1); //vorne rechts
                //            //}
                //            //else //Blickrichtung nach Westen
                //            //{
                //            //    ueberpruefpunkte_Richtung_ast[0] = (0, -1); //vorne
                //            //    ueberpruefpunkte_Richtung_ast[1] = (-1, 0); //rechts
                //            //    ueberpruefpunkte_Richtung_ast[2] = (0, 1); //hinten
                //            //    ueberpruefpunkte_Richtung_ast[3] = (1, 0); //linkss
                //            //}
                //        }
                //    }

                //    #endregion Überprüfung obenrechts, untenlinks, untenrechts, obenlinks
                //    besuchteFelder_links[besuchteFelder_links_counter] = neuerBlock;
                //    letztePosition = runner_links;
                //    runner_links = neuerBlock;
                //    Debug.WriteLine($"Letzte Posi: y:{letztePosition.pos_y},x:{letztePosition.pos_x}; Neue Posi: y:{neuerBlock.pos_y},x:{neuerBlock.pos_x}");
                //    besuchteFelder_links_counter++;
                //    counter++;
                //    #endregion
                //}

                #endregion alt
                //Überprüfung, ob in Richtung "letzteWand_Richtung_absolut" noch eine Wand ist

                (int y, int x) fortb_wand = Konvertiere_Richtung_zu_Bewegung(letzteWand_Richtung_absolut);
                bool istWandNochVorhanden = !(Ueberpruefe_existiertPunkt_runner(runner_links.pos_y + fortb_wand.y, runner_links.pos_x + fortb_wand.x));
                if (istWandNochVorhanden)
                {
                    //Bewegung im 90 Grad Winkel zur Wand nach rechts
                    //Ist der nächste Block frei oder ist eine Wand im Weg?
                    (int y, int x) fortb_weg = Konvertiere_Richtung_zu_Bewegung(richtung);
                    bool istWegFrei = Ueberpruefe_existiertPunkt_runner(runner_links.pos_y + fortb_weg.y, runner_links.pos_x + fortb_weg.x);
                    if (istWegFrei)
                    {
                        //Wenn der Weg frei ist, bewegt sich der Runner weiter
                        neuerBlock = new Block(runner_links.pos_y + fortb_weg.y, runner_links.pos_x + fortb_weg.x);
                        besuchteFelder_links[besuchteFelder_links_counter] = neuerBlock;
                        besuchteFelder_links_counter++;
                        //counter++; //Debug
                        runner_links = neuerBlock;
                    }
                    else
                    {
                        //Wenn der Weg versperrt ist, wird die Richtung der letzten Wand angepasst und die While Schleife wiederholt
                        letzteWand_Richtung_absolut = richtung;
                        richtung++;
                        if (richtung > 3)
                        {
                            richtung = 0;
                        }
                    }
                }
                else //Wand ist nicht mehr vorhanden
                {
                    //Wenn keine Wand vorhanden ist, geh in die Richtung, wo zuletzt die Wand war und drehe dich um 90 Grad in die Richtung,
                    //wo zuletzt die Wand war
                    neuerBlock = new Block(runner_links.pos_y + fortb_wand.y, runner_links.pos_x + fortb_wand.x);
                    besuchteFelder_links[besuchteFelder_links_counter] = neuerBlock;
                    besuchteFelder_links_counter++;
                    //counter++; //Debug
                    runner_links = neuerBlock;
                    richtung = letzteWand_Richtung_absolut;
                    letzteWand_Richtung_absolut = richtung - 1;
                    if (letzteWand_Richtung_absolut < 0)
                    {
                        letzteWand_Richtung_absolut = 3;
                    }
                    //Wiederholung der While Schleife
                }
            }
            Block[] ausgabe_links = Bereite_BesuchtenWegAuf(besuchteFelder_links);
            BesuchteFelder_links = ausgabe_links;
            BesuchteFelder_links_counter = ausgabe_links.Length;
            
            (int y, int x) Konvertiere_Richtung_zu_Bewegung(int Richtung)
            {
                (int y, int x) fortbewegung;
                if (Richtung == 0)
                {
                    fortbewegung = (-1, 0);
                }
                else if (Richtung == 1)
                {
                    fortbewegung = (0, 1);
                }
                else if (Richtung == 2)
                {
                    fortbewegung = (1, 0);
                }
                else
                {
                    fortbewegung = (0, -1);
                }
                return fortbewegung;
            }
        }

        private Block[] Bereite_BesuchtenWegAuf(Block[] besuchterWeg)
        {
            IEnumerable<Block> besuchterWeg_Ien= besuchterWeg.Where(x => x != null);
            List<Block> besuchterWeg_Liste = besuchterWeg_Ien.ToList();
            int besuchterWeg_Liste_index = 0;
            while (besuchterWeg_Liste_index < besuchterWeg_Liste.Count)
            {
                List<(Block block, int index)> gefundeneBloecke = new List<(Block block, int index)>();
                int gefundeneBloecke_index = 0;

                for (int i = 0; i < besuchterWeg_Liste.Count; i++)
                {
                    if (besuchterWeg_Liste[i].pos_y == besuchterWeg_Liste[besuchterWeg_Liste_index].pos_y && besuchterWeg_Liste[i].pos_x == besuchterWeg_Liste[besuchterWeg_Liste_index].pos_x)
                    {
                        gefundeneBloecke.Add((besuchterWeg_Liste[i], i));
                        gefundeneBloecke_index++;
                    }
                }


                //IEnumerable<Block> treffer = besuchterWeg_Liste.Where(block => block.pos_y == besuchterWeg_Liste[aktuellerindex].pos_y && block.pos_x == besuchterWeg_Liste[aktuellerindex].pos_x);
                //int count = treffer.Count();

                if (gefundeneBloecke.Count > 1)
                {

                    //ein Block ist mehr als einmal vorhanden
                    Block value = gefundeneBloecke[0].block;
                    int start = gefundeneBloecke[0].index;
                    int ende = gefundeneBloecke[1].index;
                    int anzahl = ende - start;
                    for (int i = 0; i < anzahl; i++)
                    {
                        besuchterWeg_Liste.RemoveAt(start + 1);
                    }
                    continue;
                }
                besuchterWeg_Liste_index++;
            }
            Block[] ausgabe_besuchterWeg = besuchterWeg_Liste.ToArray();
            return ausgabe_besuchterWeg.ToArray();
        }

        public void Resize_Update_Properties()
        {
            AnzahlDerBloecke_X = (int)(this.ClientSize.Width / App_Einstellungen.Blockgroesse);
            AnzahlDerBloecke_Y = (int)(this.ClientSize.Height / App_Einstellungen.Blockgroesse);
        }

        public void Erstelle_Maze_Rahmen(Button renderCaller)
        {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Start();
            RenderCaller = renderCaller;
            RenderCaller.Enabled = false;

            int width = ClientSize.Width - 2 * App_Einstellungen.Margin;
            int height = ClientSize.Height - 2 * App_Einstellungen.Margin;
            int blockgroesse = App_Einstellungen.Blockgroesse;

            int anzahl_bloecke_x = Convert.ToInt32(width / blockgroesse);
            int anzahl_bloecke_y = Convert.ToInt32(height / blockgroesse);

            bool[][] maze = new bool[anzahl_bloecke_y][];
            //Rectangle[][] rectangles = new Rectangle[anzahl_bloecke_y][];
            Block[] erstellreihenfolge = new Block[anzahl_bloecke_y * anzahl_bloecke_x];
            for (int i = 0; i < anzahl_bloecke_y; i++)
            {
                maze[i] = new bool[anzahl_bloecke_x];
                //rectangles[i] = new Rectangle[anzahl_bloecke_x];
            }



            // Erstellen des Rahmens
            for (int i = 0; i < anzahl_bloecke_x; i++) //oberer Rahmen
            {
                //if (maze[0][i] == false)
                //{
                //    maze[0][i] = true;
                //    rectangles[0][i] = new Rectangle(Besorge_Punkt(0, i), new Size(blockgroesse, blockgroesse));
                //    erstellreihenfolge[BereitsBelegteBloecke] = new Block(0, i);
                //    BereitsBelegteBloecke++;
                //}
                Erstelle_Block(0, i);
            }
            for (int i = 0; i < anzahl_bloecke_x; i++) //unterer Rahmen
            {
                //if (maze[anzahl_bloecke_y - 1][i] == false)
                //{
                //    maze[anzahl_bloecke_y - 1][i] = true;
                //    rectangles[anzahl_bloecke_y - 1][i] = new Rectangle(Besorge_Punkt(anzahl_bloecke_y - 1, i), new Size(App_Einstellungen.Blockgroesse, App_Einstellungen.Blockgroesse));
                //    erstellreihenfolge[BereitsBelegteBloecke] = new Block(anzahl_bloecke_y - 1, i);
                //    BereitsBelegteBloecke++;
                //}
                Erstelle_Block(anzahl_bloecke_y - 1, i);
            }
            for (int i = 0; i < anzahl_bloecke_y; i++) //Rahmen links + rechts
            {
                //if (maze[i][0] == true)
                //{
                //    maze[i][0] = true;
                //    rectangles[i][0] = new Rectangle(Besorge_Punkt(i, 0), new Size(App_Einstellungen.Blockgroesse, App_Einstellungen.Blockgroesse));
                    
                    
                //    maze[i][anzahl_bloecke_x - 1] = true;
                //    rectangles[i][anzahl_bloecke_x - 1] = new Rectangle(Besorge_Punkt(i, anzahl_bloecke_x - 1), new Size(App_Einstellungen.Blockgroesse, App_Einstellungen.Blockgroesse));

                //}
                Erstelle_Block(i, 0);
                Erstelle_Block(i, anzahl_bloecke_x - 1);
            }
            Maze_Rohdaten = maze;
            //Maze_Rectangles = rectangles;
            Maze_ErstellReihenfolge = erstellreihenfolge;
            BereitsBelegteBloecke_EndeRahmen = BereitsBelegteBloecke;

            Erstelle_Maze_Inhalt();

            void Erstelle_Block(int y, int x)
            {
                if (maze[y][x] == false)
                {
                    maze[y][x] = true;
                    //rectangles[y][x] = new Rectangle(Besorge_Punkt(y, x), new Size(blockgroesse, blockgroesse));
                    erstellreihenfolge[BereitsBelegteBloecke] = new Block(y, x);
                    BereitsBelegteBloecke++;
                }
            }
        }

        private Point Besorge_Punkt(int y, int x)
        {
            return new Point(x * App_Einstellungen.Blockgroesse + App_Einstellungen.Margin, y * App_Einstellungen.Blockgroesse + App_Einstellungen.Margin);
        }
        #endregion Methoden
    }
}
