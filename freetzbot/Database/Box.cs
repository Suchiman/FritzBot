using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FritzBot.Core;

namespace FritzBot.Database
{
    public class Box
    {
        public virtual Int64 Id { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string FullName { get; set; }
        public virtual ICollection<BoxRegexPattern> RegexPattern { get; set; }

        /// <summary>
        /// Überprüft mit den gegebenen Daten ob der input dieser Box entspricht
        /// </summary>
        public bool Matches(string input)
        {
            return (ShortName.Equals(input, StringComparison.OrdinalIgnoreCase) || FullName.Equals(input, StringComparison.OrdinalIgnoreCase) || RegexPattern.Select(x => x.Pattern).Any(x => Regex.Match(input, x, RegexOptions.IgnoreCase).Success));
        }

        /// <summary>
        /// Fügt einen Regulären Ausdruck zu den Erkennungsmustern hinzu. Doppelte Einträge werden automatisch entfernt
        /// </summary>
        public void AddRegex(params string[] pattern)
        {
            RegexPattern.AddRange(pattern.Except(RegexPattern.Select(x => x.Pattern)).Select(x => new BoxRegexPattern { Pattern = x }));
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
            return ShortName == (obj as Box)?.ShortName;
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