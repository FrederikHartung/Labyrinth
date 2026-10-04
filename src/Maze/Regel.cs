using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public class Regel
    {
        List<(int vor, int rechts)> List_abstaende = new List<(int y, int x)>();

        public void AddPunkt(int vor, int rechts)
        {
            List_abstaende.Add((vor, rechts));
        }
    }
}
