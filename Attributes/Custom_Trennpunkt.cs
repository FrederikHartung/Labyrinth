using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    [System.AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// Jede Property, die dieses Attribute hat, bekommt beim setzen dieses Wertes in einen Textfeld diesen Wert als Einheit hinzugefügt
    /// </summary>
    public class Custom_Trennpunkt : Attribute
    {
        public Custom_Trennpunkt(int abstand, string trennsymbol)
        {
            Abstand = abstand;
            Trennsymbol = trennsymbol;
        }

        public int Abstand { get; set; }
        public string Trennsymbol { get; set; }
    }
}
