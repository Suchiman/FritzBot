using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.DataModel.LegacyModels
{
    public class Box
    {
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public List<string> RegexPattern { get; set; }

        public Box()
        {
            RegexPattern = new List<string>();
        }

        /// <summary>
        /// Überprüft mit den gegebenen Daten ob der input dieser Box entspricht
        /// </summary>
        public bool Matches(string input)
        {
            return ((ShortName == input) || (FullName == input) || RegexPattern.Any(x => Regex.Match(input, (string)x).Success));
        }

        /// <summary>
        /// Fügt einen Regulären Ausdruck zu den Erkennungsmustern hinzu. Doppelte Einträge werden automatisch entfernt
        /// </summary>
        public void AddRegex(params string[] pattern)
        {
            RegexPattern.AddRange(pattern);
            RegexPattern = RegexPattern.Distinct().ToList();
        }

        /// <summary>
        /// Entfernt einen Regulären Ausdruck aus den Erkennungsmustern
        /// </summary>
        public void RemoveRegex(string pattern)
        {
            RegexPattern.Remove(pattern);
        }

        public override bool Equals(object obj)
        {
            if (obj is Box)
            {
                return ShortName == ((Box)obj).ShortName;
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
            if ((object)daten1 == null && (object)daten2 == null)
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
            if ((object)daten1 == null && (object)daten2 == null)
            {
                return false;
            }
            return daten1.ShortName != daten2.ShortName;
        }
    }
}