using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public static class App_Einstellungen
    {
        [Custom_Ignore(true)]
        public static string Speicherort_Einstellungen { get; set; }

        public static int Margin { get; set; } = 50;
        public static int Blockgroesse { get; set; } = 50;
        public static int Gewichtung_Rahmen { get; set; } = 80;

        private static int breite = 500;
        public static int Breite {
            get
            {
                return breite;
            }
            set
            {
                Helper.Mainform.Width = value;
                breite = value;
            }
        }
        private static int hoehe = 500;
        public static int Hoehe {
            get
            {
                return hoehe;
            }
            set
            {
                Helper.Mainform.Height = value;
                hoehe = value;
            }
        }

    }
}
