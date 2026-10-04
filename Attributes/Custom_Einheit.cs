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
    public class Custom_Einheit : Attribute
    {
        public Custom_Einheit(string einheit)
        {
            Einheit = einheit;
        }

        public string Einheit { get; set; }

    }
}
