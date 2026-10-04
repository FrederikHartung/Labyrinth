using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Labyrinth
{
    public static class StringHelper
    {
        public static string BesorgeSubstring(string imput, string startUebereinstimmung, string endUebereinstimmung)
        {
            int indexStart = imput.IndexOf(startUebereinstimmung) + 1;
            int indexEnde = imput.IndexOf(endUebereinstimmung, indexStart);
            string ausgabe = imput.Substring(indexStart, indexEnde - indexStart);
            return ausgabe;
        }

        public static string BesorgeSubstring(string imput, string startUebereinstimmung, string endUebereinstimmung, ref int ref_startIndex)
        {
            int indexStart = imput.IndexOf(startUebereinstimmung, ref_startIndex) + 1;
            ref_startIndex = indexStart;
            int indexEnde = imput.IndexOf(endUebereinstimmung, indexStart);
            string ausgabe = imput.Substring(indexStart, indexEnde - indexStart);
            return ausgabe;
        }

        public static string BesorgeSubstring(string imput, string startUebereinstimmung, string endUebereinstimmung, ref int ref_startIndex, ref int ref_endIndex)
        {
            int indexStart = imput.IndexOf(startUebereinstimmung, ref_startIndex) + 1;
            if (indexStart == 0)
            {
                return null;
            }
            ref_startIndex = indexStart;
            int indexEnde = imput.IndexOf(endUebereinstimmung, indexStart);
            ref_endIndex = indexEnde;
            string ausgabe = imput.Substring(indexStart, indexEnde - indexStart);
            return ausgabe;
        }

        public static List<string> BesorgeAlleSubstrings(string imput, string startUebereinstimmung, string endUebereinstimmung)
        {
            int start = 0;
            int end = 0;
            List<string> stringListe = new List<string>();

            while (true)
            {
                string line = BesorgeSubstring(imput, startUebereinstimmung, endUebereinstimmung, ref start, ref end);
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                start = end + 1;
                stringListe.Add(line);
            }
            return stringListe;
        }

        public static string Besorge_OrdnerPfad_Von_DateiPfad(string filepath)
        {
            string ausgabe = filepath.Substring(0, filepath.LastIndexOf("\\") + 1);
            return ausgabe;
        }

        public static string Besorge_DateiName_Von_DateiPfad(string filepath)
        {
            string ausgabe = filepath.Substring(filepath.LastIndexOf("\\") + 1);
            return ausgabe;
        }

        public static T GetTfromString<T>(string mystring)
        {
            var foo = TypeDescriptor.GetConverter(typeof(T));
            return (T)(foo.ConvertFromInvariantString(mystring));
        }
    }
}
