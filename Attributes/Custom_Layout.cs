using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    /// <summary>
    /// Jede Property, die dieses Attribute hat, wird bei der Auto Layout Generierung berücksichtigt
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property)]
    public class Custom_Layout : System.Attribute
    {
    }
}
