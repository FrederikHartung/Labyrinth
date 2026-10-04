
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Labyrinth
{
    public partial class Form_Mainform : Form
    {
        public Form_Mainform()
        {
            InitializeComponent();

            Helper.Mainform = this;
            string speicherort = ExportImportHelper.Besorge_Default_Speicherort(true);
            ExportImportHelper.Erstelle_OrdnerStruktur_wennNoetig(speicherort);
            App_Einstellungen.Speicherort_Einstellungen = speicherort;

            var letzterImport = ExportImportHelper.Besorge_letztenExport(false, "");
            if (letzterImport.fehler == true && letzterImport.filepath == "")
            {
                MessageBox.Show("Die Einstellungsdatei wurde nicht gefunden. Deswegen wurde das Programm mit den Standard Einstellungen gestartet.");
            }

            if (letzterImport.filepath != "")
            {
                string import = ExportImportHelper.Import_Static_Objekt(letzterImport.filepath);
            }

            ErstelleLayout();
        }

        public Color Actionbar_BackgroundColor { get; set; } = Color.White;
        public Color Canvas_BackgroundColor { get; set; } = Color.LightGray;

        public Panel Actionbar { get; set; }
        public Canvas _Canvas { get; set; }
        public int Margin_Actionbar { get; set; } = 10;
        public bool ZeigePerformence { get; set; } = false;


        private void ErstelleLayout()
        {
            Erstelle_Actionbar();
            Erstelle_Canvas();
            Resize_Frame();
        }

        private void Erstelle_Actionbar()
        {
            Panel panel = new Panel();
            panel.Location = new Point(0, 0);
            panel.Height = (int)(this.ClientSize.Height * 0.1);
            panel.Width = this.ClientSize.Width;
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Name = "Actionbar";
            panel.BackColor = Actionbar_BackgroundColor;
            this.Controls.Add(panel);

            Button button_oeffneEinstellungen = new Button();
            button_oeffneEinstellungen.Text = "Öffne Einstellugen";
            button_oeffneEinstellungen.Click += button_oeffneEinstellungen_Click;
            panel.Controls.Add(button_oeffneEinstellungen);

            Button button_zeichneLabyrinth = new Button();
            button_zeichneLabyrinth.Text = "Zeichne Labyrinth";
            button_zeichneLabyrinth.Click += button_zeicheLabyrinth_click;
            panel.Controls.Add(button_zeichneLabyrinth);

            Button button_zuruecksetzen= new Button();
            button_zuruecksetzen.Text = "Zurücksetzen";
            button_zuruecksetzen.Click += button_zuruecksetzen_click;
            panel.Controls.Add(button_zuruecksetzen);

            //Button button_test = new Button();
            //button_test.Text = "Test";
            //button_test.Click += Button_test_Click; ;
            //panel.Controls.Add(button_test);

            Button button_performence = new Button();
            button_performence.Text = "Zeige Performence";
            button_performence.Click += Button_performence_Click;
            panel.Controls.Add(button_performence);

            Actionbar = panel;
        }

        private void Button_performence_Click(object sender, EventArgs e)
        {
            Form_Performence form = new Form_Performence((Button)sender);
            form.Show();
        }

        private void Button_test_Click(object sender, EventArgs e)
        {
            Form_Test form = new Form_Test((Button)sender);
            form.Show();
        }

        public void Erstelle_Canvas()
        {
            Canvas canvas = new Canvas(this.ClientSize.Width, this.ClientSize.Height - Actionbar.ClientSize.Height);
            this.Controls.Add(canvas);
            this._Canvas = canvas;
        }

        private void button_zeicheLabyrinth_click(object sender, EventArgs e)
        {
            SetzteMazeZurueck();
            if (((Button)sender).Enabled)
            {
                if (_Canvas == null)
                {
                    Erstelle_Canvas();
                }
                _Canvas.Erstelle_Maze_Rahmen((Button)sender);
                //Danach wird das Paint Event des Canvas ausgelöst
            }
        }

        private void button_oeffneEinstellungen_Click(object sender, EventArgs e)
        {
            Form_Einstellungen form = new Form_Einstellungen((Button)sender);
            form.Show();
        }

        private void button_zuruecksetzen_click(object sender, EventArgs e)
        {
            SetzteMazeZurueck();
        }

        private void SetzteMazeZurueck()
        {
            _Canvas.Maze_Rohdaten = null;
            _Canvas.Maze_ErstellReihenfolge = null;
            _Canvas.BereitsBelegteBloecke = 0;
            _Canvas.BereitsBelegteBloecke_EndeRahmen = 0;
            _Canvas.IstGezeichnet = false;
            _Canvas.Refresh();
            _Canvas.BesuchteFelder_links = null;
            _Canvas.BesuchteFelder_rechts = null;
            _Canvas.BesuchteFelder_rechts_counter = 0;
            _Canvas.BesuchteFelder_links_counter = 0;
        }


        public void Resize_Frame()
        {
            if (Actionbar == null || _Canvas == null)
            {
                return;
            }

            if (Actionbar.Controls.Count == 0)
            {
                return;
            }

            #region Actionbar
            Actionbar.Height = (int)(this.ClientSize.Height * 0.1);
            Actionbar.Width = this.ClientSize.Width;
            Panel panel = Actionbar;

            int margin = Margin_Actionbar;
            int height = panel.ClientSize.Height - 2 * margin;
            int width = (int)(panel.ClientSize.Width - 2 * margin - (panel.Controls.Count - 1) * margin);
            Size size = new Size((int)(width / panel.Controls.Count), height);
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                ((Button)panel.Controls[i]).Size = size;
                //((Button)panel.Controls[i]).Margin = new Padding(0);
                ((Button)panel.Controls[i]).Location = new Point(panel.Location.X + size.Width * i + Margin_Actionbar + i * Margin_Actionbar, panel.Location.Y + Margin_Actionbar);
            }
            #endregion

            #region Canvas
            _Canvas.Width = this.ClientSize.Width;
            _Canvas.Height = this.ClientSize.Height - Actionbar.Height;
            //Form form_performence = Helper.Aktive_Forms.FirstOrDefault(x => x.GetType() == typeof(Form_Performence));
            //if (form_performence != null)
            //{
            //    (form_performence as Form_Performence).Berechne_AnzahlAnBloecken();
            //}
            _Canvas.Location = new Point(0, Actionbar.Height);
            #endregion
        }

        private void Form_Mainform_Resize(object sender, EventArgs e)
        {
            Resize_Frame();
        }
    }
}
