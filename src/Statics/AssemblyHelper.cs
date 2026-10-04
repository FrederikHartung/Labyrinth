using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    public static class AssemblyHelper
    {
        public static string BesorgeAssemblyOrdnerPfad()
        {
            return $@"{Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf("\\") + 1)}";
        }

        public static Struct_AssemblyQualifiedName Loese_AssemblyQualifiedName_auf(string assemblyQualifiedName)
        {
            Struct_AssemblyQualifiedName qualifiedName = new Struct_AssemblyQualifiedName();
            int posi_start = assemblyQualifiedName.IndexOf("\"");
            int posi_end = assemblyQualifiedName.IndexOf(",");
            qualifiedName.Klasse_FullName = assemblyQualifiedName.Substring(posi_start + 1, posi_end - posi_start - 1);
            qualifiedName.Assembly_Name = StringHelper.BesorgeSubstring(assemblyQualifiedName, ",", ",", ref posi_start, ref posi_end).Trim();
            posi_start++;
            qualifiedName.Assembly_Version = StringHelper.BesorgeSubstring(assemblyQualifiedName, ",", ",", ref posi_start, ref posi_end).Trim();
            posi_start++;
            qualifiedName.Culture = StringHelper.BesorgeSubstring(assemblyQualifiedName, ",", ",", ref posi_start, ref posi_end).Trim();
            posi_start++;
            qualifiedName.PublicKeyToken = assemblyQualifiedName.Substring(assemblyQualifiedName.LastIndexOf(",") + 1).Trim();

            return qualifiedName;
        }

        public struct Struct_AssemblyQualifiedName
        {
            public string Klasse_FullName { get; set; }
            public string Assembly_Name { get; set; }
            public string Assembly_Version { get; set; }
            public string Culture { get; set; }
            public string PublicKeyToken { get; set; }
        }

    }
}
