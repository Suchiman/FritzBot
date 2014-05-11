using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.Database
{
    public class Box
    {
        public Int64 Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public List<BoxRegexPattern> RegexPattern { get; set; }

        public Box()
        {
            RegexPattern = new List<BoxRegexPattern>();
        }

        /// <summary>
        /// Überprüft mit den gegebenen Daten ob der input dieser Box entspricht
        /// </summary>
        public bool Matches(string input)
        {
            return ((ShortName == input) || (FullName == input) || RegexPattern.Select(x => x.Pattern).Any(x => Regex.Match(input, x).Success));
        }

        /// <summary>
        /// Fügt einen Regulären Ausdruck zu den Erkennungsmustern hinzu. Doppelte Einträge werden automatisch entfernt
        /// </summary>
        public void AddRegex(params string[] pattern)
        {
            RegexPattern.AddRange(pattern.Select(x => new BoxRegexPattern { Pattern = x }));
            RegexPattern = RegexPattern.Distinct().ToList();
        }

        /// <summary>
        /// Entfernt einen Regulären Ausdruck aus den Erkennungsmustern
        /// </summary>
        public void RemoveRegex(string pattern)
        {
            RegexPattern.RemoveAll(x => x.Pattern == pattern);
        }

        public override bool Equals(object obj)
        {
            var box = obj as Box;
            if (box != null)
            {
                return ShortName == box.ShortName;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ShortName.GetHashCode();
        }

        public override string ToString()
        {
            return ShortName;
        }

        public static bool operator ==(Box daten1, Box daten2)
        {
            if ((object)daten1 == null ^ (object)daten2 == null)
            {
                return false;
            }
            if ((object)daten1 == null)
            {
                return true;
            }
            return daten1.Equals(daten2);
        }

        public static bool operator !=(Box daten1, Box daten2)
        {
            if ((object)daten1 == null ^ (object)daten2 == null)
            {
                return true;
            }
            if ((object)daten1 == null)
            {
                return false;
            }
            return daten1.ShortName != daten2.ShortName;
        }
    }
}