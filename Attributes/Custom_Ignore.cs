using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    [System.AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// Jede Property, die dieses Attribute hat, wird bei der Auto Layout Generierung ausgeschlossen
    /// </summary>
    public class Custom_Ignore : System.Attribute
    {
        public Custom_Ignore(bool ignore)
        {
            Ignore = ignore;
        }
        public bool Ignore { get; set; }
    }
}
