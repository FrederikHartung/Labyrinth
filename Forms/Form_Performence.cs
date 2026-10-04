
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    public partial class Form_Performence : Base_Form_Core
    {
        public Form_Performence(Button button) : base(button)
        {
            InitializeComponent();
            this.Name = "Performence";
            //Layout_Margin = 50;
            Erstelle_Layout();
            LeseAktuelleConfiguration();
            this.FormClosing += Form_Performence_FormClosing;
        }

        private void Form_Performence_FormClosing(object sender, FormClosingEventArgs e)
        {
            Helper.Mainform._Canvas.ZuRendernderErstellfortschritt = Helper.Mainform._Canvas.BereitsBelegteBloecke;
        }

        //public int Layout_Margin { get; set; }

        private int bloecke_gesammt = 0;
        [Custom_Layout]
        [Custom_Trennpunkt(3, ".")]
        [DisplayName("Größe des Labyrinthes ( x * y )")]
        public int Bloecke_gesammt
        {
            get { return bloecke_gesammt; }
            set { bloecke_gesammt = value; Setzte_Textbox_Wert(nameof(Bloecke_gesammt), bloecke_gesammt); }
        }

        private int bloecke_Richtung_X = 0;
        [Custom_Layout]
        [DisplayName("Blöcke Richtung X")]
        public int Bloecke_Richtung_X
        {
            get { return bloecke_Richtung_X; }
            set { bloecke_Richtung_X = value;
                Berechne_AnzahlAnBloecken();
                Setzte_Textbox_Wert(nameof(Bloecke_Richtung_X), bloecke_Richtung_X); }
        }

        private int bloecke_Richtung_Y = 0;
        [Custom_Layout]
        [DisplayName("Blöcke Richtung X")]
        public int Bloecke_Richtung_Y
        {
            get { return bloecke_Richtung_Y; }
            set { bloecke_Richtung_Y = value;
                Berechne_AnzahlAnBloecken();
                Setzte_Textbox_Wert(nameof(Bloecke_Richtung_Y), bloecke_Richtung_Y); }
        }

        private double dauer_der_Berechnung = 0f;
        [Custom_Layout]
        [Custom_Einheit("Sekunden")]
        [DisplayName("Dauer der Berechnung")]
        public double Dauer_der_Berechnung
        {
            get { return dauer_der_Berechnung; }
            set { dauer_der_Berechnung = value; Setzte_Textbox_Wert(nameof(Dauer_der_Berechnung), dauer_der_Berechnung); }
        }

        private int anzahl_erstellterBloecke = 0;
        [Custom_Layout]
        [Custom_Einheit("Blöcke")]
        [DisplayName("Anzahl der erstellten Blöcke")]
        [Custom_Trennpunkt(3, ".")]
        public int Anzahl_erstellterBloecke
        {
            get { return anzahl_erstellterBloecke; }
            set { anzahl_erstellterBloecke = value; Setzte_Textbox_Wert(nameof(Anzahl_erstellterBloecke), anzahl_erstellterBloecke); }
        }

        public TrackBar Trackbar_Erstellreihenfolge { get; set; }

        //private void Setzte_Textbox_Wert<T>(string propertyName, ref T dummy, T value)
        //{
        //    
        //    value.GetType().GetProperty(propertyName).SetValue(dummy, value);
        //}

        public void Berechne_AnzahlAnBloecken()
        {
            Bloecke_gesammt = Bloecke_Richtung_X * Bloecke_Richtung_Y;
        }

        private void LeseAktuelleConfiguration()
        {
            Control canvas = Helper.Mainform.Controls.Find(nameof(Canvas), false).FirstOrDefault(x => x.GetType() == typeof(Canvas));
            if (canvas != null)
            {
                Bloecke_Richtung_X = ((Canvas)canvas).AnzahlDerBloecke_X;
                Bloecke_Richtung_Y = ((Canvas)canvas).AnzahlDerBloecke_Y;
                Berechne_AnzahlAnBloecken();
            }
            else
            {
                Helper.Mainform.Erstelle_Canvas();
                LeseAktuelleConfiguration();
            }
            //else
            //{
            //    MessageBox.Show($"Interner Fehler: Die aktuelle Konfiguration konnte nicht geladen werden in der {this.GetType().Name}" +
            //        $" Klasse/ Methode: LeseAktuelleConfiguration()");
            //}
        }

        private void Setzte_Textbox_Wert(string propertyName, object value)
        {
            TextBox box = (TextBox)Controls.Find(propertyName, false).Single(x => x.Name == propertyName);
            string ausgabe = value.ToString();

            Attribute attribute_trennpunk = this.GetType().GetProperty(propertyName).GetCustomAttribute(typeof(Custom_Trennpunkt));
            if (attribute_trennpunk != null)
            {
                Custom_Trennpunkt custom = (Custom_Trennpunkt)attribute_trennpunk;
                int anzahl = (ausgabe.Length - 1) / custom.Abstand; // -1, damit der Trennpunkt erst ab 1000 erstellt wird

                for (int i = 1; i <= anzahl; i++)
                {
                    ausgabe = ausgabe.Insert((ausgabe.Length) - custom.Abstand * i - (i - 1), custom.Trennsymbol);
                }
            }

            Attribute attribute_einheit = this.GetType().GetProperty(propertyName).GetCustomAttribute(typeof(Custom_Einheit));
            if (attribute_einheit != null)
            {
                Custom_Einheit custom = (Custom_Einheit)attribute_einheit;
                ausgabe += " " + custom.Einheit;
            }

            box.Text = ausgabe;
        }

        private void Erstelle_Layout()
        {
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.GetCustomAttribute(typeof(Custom_Layout)) != null).ToArray();
            int zuerstellendeZeilen = properties.Count() + 1; //+1 für Trackbar
            this.Height = zuerstellendeZeilen * 50;
            this.Width = 500;

            int naechste_posi_y = 0;

            foreach (PropertyInfo prop in properties)
            {
                Label label = new Label();
                Attribute displayname = prop.GetCustomAttribute(typeof(System.ComponentModel.DisplayNameAttribute));
                if (displayname is DisplayNameAttribute attribute)
                {
                    label.Text = attribute.DisplayName;
                }
                else
                {
                    label.Text = prop.Name;
                }
                (Point point, Size size) toupel = BesorgeNewXY( ref zuerstellendeZeilen, ref naechste_posi_y);
                label.Location = toupel.point;
                label.Size = toupel.size;
                this.Controls.Add(label);

                TextBox textbox = new TextBox();
                textbox.Name = prop.Name;
                textbox.Text = prop.GetValue(this).ToString();
                textbox.Location = new Point(Convert.ToInt32((double)ClientSize.Width / 2), toupel.point.Y);
                textbox.Size = toupel.size;
                textbox.ReadOnly = true;
                this.Controls.Add(textbox);
            }

            (Point point, Size size) toupel2 = BesorgeNewXY(ref zuerstellendeZeilen, ref naechste_posi_y);
            Trackbar_Erstellreihenfolge = new TrackBar();
            Trackbar_Erstellreihenfolge.Name = "Trackbar_Erstellreihenfolge";
            Trackbar_Erstellreihenfolge.Text = "Erstellreihenfolge";
            Trackbar_Erstellreihenfolge.Enabled = false;
            Trackbar_Erstellreihenfolge.Location = toupel2.point;
            Trackbar_Erstellreihenfolge.Size = new Size(toupel2.size.Width * 2, toupel2.size.Height);
            Trackbar_Erstellreihenfolge.ValueChanged += Trackbar_Erstellreihenfolge_ValueChanged;
            this.Controls.Add(Trackbar_Erstellreihenfolge);
            #region
            //Button button2 = new Button();
            //button2.Name = "button_speichern";
            //button2.Click += button_speichereEinstellungen_Click;
            //button2.Text = "Speichern";
            //button2.Location = new Point(0, counter * sizeY + LayoutMargin);
            //button2.Size = new Size((int)(size.Width * 2 / 3), size.Height); ;
            //this.Controls.Add(button2);

            //Button button = new Button();
            //button.Name = "button_ok";
            //button.Click += Button_Click;
            //button.Text = "OK";
            //button.Location = new Point((int)(sizeX * 2 / 3), counter * sizeY + LayoutMargin);
            //button.Size = new Size((int)(size.Width * 2 / 3), size.Height); ;
            //this.Controls.Add(button);

            //Button button3 = new Button();
            //button3.Name = "button_abbrechen";
            //button3.Click += Button3_Click; ;
            //button3.Text = "Abbrechen";
            //button3.Location = new Point(2 * (int)(sizeX * 2 / 3), counter * sizeY + LayoutMargin);
            //button3.Size = new Size((int)(size.Width * 2 / 3), size.Height); ;
            //this.Controls.Add(button3);
            #endregion
        }

        private void Trackbar_Erstellreihenfolge_ValueChanged(object sender, EventArgs e)
        {
            if (Helper.Mainform._Canvas != null)
            {
                Helper.Mainform._Canvas.Refresh();
            }
        }

        private (Point point, Size size) BesorgeNewXY(ref int anzahl_nochZuErstellen, ref int naechste_posi_y)
        {
            int y_neu = Convert.ToInt32(((double)(ClientSize.Height - naechste_posi_y)) / anzahl_nochZuErstellen);
            Size groesse = new Size(Convert.ToInt32((double)ClientSize.Width / 2), y_neu);
            Point punkt = new Point(0, Convert.ToInt32(naechste_posi_y));
            naechste_posi_y += y_neu;
            anzahl_nochZuErstellen--;
            return (punkt, groesse);
        }
    }
}
