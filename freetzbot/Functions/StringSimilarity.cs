using System;
using System.Collections.Generic;

namespace FritzBot.Functions
{
    /// <summary>
    /// Bestimmt mithilfe der Levenshtein-Distance die Ähnlichkeit von 2 Strings
    /// </summary>
    public static class StringSimilarity
    {
        /// <summary>
        /// Ein Cache zur Beschleunigung
        /// </summary>
        static Dictionary<String, int> memo = new Dictionary<String, int>();

        /// <summary>
        /// Führt die Berechnung durch
        /// </summary>
        private static int LevenshteinDistance(Char[] str1, int i, int len1, Char[] str2, int j, int len2)
        {
            String key = String.Join(",", new string[] { i.ToString(), len1.ToString(), j.ToString(), len2.ToString() });
            if (memo.ContainsKey(key)) return memo[key];

            if (len1 == 0) return len2;
            if (len2 == 0) return len1;
            int cost = 0;
            if (str1[i] != str2[j]) cost = 1;

            int dist = Math.Min(LevenshteinDistance(str1, i + 1, len1 - 1, str2, j, len2) + 1, LevenshteinDistance(str1, i, len1, str2, j + 1, len2 - 1) + 1);
            dist = Math.Min(dist, LevenshteinDistance(str1, i + 1, len1 - 1, str2, j + 1, len2 - 1) + cost);
            memo[key] = dist;
            return dist;
        }

        /// <summary>
        /// Vergleicht 2 Strings und gibt die Anzahl der Änderungen zurück die notwendig sind um <paramref name="str1"/> in <paramref name="str2"/> zu Transformieren
        /// </summary>
        /// <param name="str1">Erster String</param>
        /// <param name="str2">Zweiter String</param>
        /// <param name="IgnoreCase">Groß / Kleinschreibung ignorieren</param>
        public static int Compare(String str1, String str2, bool IgnoreCase)
        {
            memo = new Dictionary<String, int>();
            if (IgnoreCase)
            {
                str1 = str1.ToLower();
                str2 = str2.ToLower();
            }
            return LevenshteinDistance(str1.ToCharArray(), 0, str1.Length, str2.ToCharArray(), 0, str2.Length);
        }

        /// <summary>
        /// Vergleicht 2 Strings und gibt die Ähnlichkeit in Prozent zurück
        /// </summary>
        /// <param name="str1">Erster String</param>
        /// <param name="str2">Zweiter String</param>
        /// <param name="IgnoreCase">Groß / Kleinschreibung ignorieren</param>
        public static double ComparePercent(String str1, String str2, bool IgnoreCase)
        {
            double distance = Compare(str1, str2, IgnoreCase);
            double maxlength = Math.Max(str1.Length, str2.Length);
            return (100d - ((distance / maxlength) * 100d));
        }
    }
}