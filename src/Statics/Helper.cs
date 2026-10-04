using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth
{
    internal static class Helper
    {
        public static Form_Mainform Mainform { get; set; }
        public static List<Form> Aktive_Forms { get; set; } = new List<Form>();
    }
}
