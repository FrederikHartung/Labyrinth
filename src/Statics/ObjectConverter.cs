using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public static class ObjectConverter
    {
        public static System.Collections.IList CreateList(Type myType)
        {
            Type genericListType = typeof(List<>).MakeGenericType(myType);
            return (System.Collections.IList)Activator.CreateInstance(genericListType);
        }
    }
}
