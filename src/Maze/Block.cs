using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public class Block
    {
        public Block(int y, int x)
        {
            pos_x = x;
            pos_y = y;
        }

        public int pos_x;
        public int pos_y;
        //public System.Drawing.Rectangle rectangle;
    }
}
