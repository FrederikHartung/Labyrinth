using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    public partial class Form_Test : Base_Form_Core
    {
        public Form_Test(Button button) : base(button)
        {
            this.InitializeComponent();
            //numericUpDown_bloecke.DecimalPlaces = 0;
            //numericUpDown_bloecke.Increment = 5;
            //numericUpDown_bloecke.Value = 5;
            ////Erstelle_Regelliste();
            //this.numericUpDown_rahmen.Value = 80;
            //this.numericUpDown_rahmen.Increment = 5;
            #region
            //this.Width = 800;
            //this.Height = 800;
            //this.BackColor = Color.Red;
            //int anzahl_zuErstellen = 16;
            //double letztePosi_Y = 0;
            //for (int i = 0; i < anzahl_zuErstellen; i++)
            //{
            //    Panel panel = new Panel();
            //    if ((i + 2) % 2 == 0)
            //    {
            //        panel.BackColor = Color.DarkBlue;
            //    }
            //    else
            //    {
            //        panel.BackColor = Color.LightGray;
            //    }

            //    double y_neu = Math.Round((ClientSize.Height - letztePosi_Y) / (anzahl_zuErstellen - i));
            //    panel.Size = new Size(this.ClientSize.Width, Convert.ToInt32(y_neu));

            //    if (i == 0)
            //    {
            //        panel.Location = new Point(0, 0);
            //    }
            //    else
            //    {
            //        panel.Location = new Point(0, Convert.ToInt32(letztePosi_Y));
            //    }

            //    letztePosi_Y += y_neu;
            //    this.Controls.Add(panel);
            //}
            ////this.Refresh();
            #endregion
        }

        private void Erstelle_Regelliste()
        {
            Regel regel = new Regel();
            regel.AddPunkt(1, 0);
            regelListe.Add(regel);

            Regel regel2 = new Regel();
            regel2.AddPunkt(2, 0);
            regelListe.Add(regel2);

            Regel regel3 = new Regel();
            regel3.AddPunkt(2, 1);
            regel3.AddPunkt(2, -1);
            regelListe.Add(regel3);
        }

        public List<Regel> regelListe = new List<Regel>();
        public int Rahmen_Gewichtung { get; set; }


       

        //private void button_ErstelleBlocks_Click(object sender, EventArgs e)
        //{
        //    Rahmen_Gewichtung = (int)numericUpDown_rahmen.Value;
        //    Berechne_Maze_Inhalt();
        //}

   }
}
