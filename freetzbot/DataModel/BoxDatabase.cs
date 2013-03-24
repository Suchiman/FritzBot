using FritzBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FritzBot.DataModel
{
    public class BoxDatabase : PluginBase
    {
        private static BoxDatabase instance;
        private List<Box> Boxen;

        public static BoxDatabase GetInstance()
        {
            if (instance == null)
            {
                instance = new BoxDatabase();
            }
            return instance;
        }

        public BoxDatabase()
        {
            Boxen = new DBProvider().Query<Box>().ToList();
        }

        /// <summary>
        /// Fügt eine neue Box hinzu
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box, z.B. 7270v2</param>
        /// <param name="FullName">Der vollständige Name</param>
        /// <param name="RegexPattern">Ein oder mehrere Reguläre Ausdrücke um die Box zu erkennen</param>
        public Box AddBox(string ShortName, string FullName, params string[] RegexPattern)
        {
            if (GetBoxByShortName(ShortName) != null)
            {
                throw new Exception("Es gibt bereits eine Box mit gleichem ShortName");
            }
            Box box = new Box()
            {
                ShortName = ShortName,
                FullName = FullName,
                RegexPattern = new List<string>(RegexPattern)
            };
            using (DBProvider db = new DBProvider())
            {
                db.SaveOrUpdate(box);
            }
            Boxen.Add(box);
            return box;
        }

        /// <summary>
        /// Gibt die Box mit entsprechendem ShortName zurück
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box</param>
        public Box GetBoxByShortName(string ShortName)
        {
            return GetBoxen().FirstOrDefault(x => x.ShortName == ShortName);
        }

        /// <summary>
        /// Versucht die zum input passende Box zu finden und den ShortName zurückzugeben. Andernfalls ist die Rückgabe der input
        /// </summary>
        /// <param name="input">Ein string anhand dessen versucht werden soll die passende Box zu finden</param>
        public string GetShortName(string input)
        {
            Box ToFind;
            if (TryFindExactBox(input, out ToFind))
            {
                return ToFind.ShortName;
            }
            return input;
        }

        /// <summary>
        /// Versucht die zum input passende Box zu finden und den ShortName zurückzugeben.
        /// </summary>
        /// <param name="input">Ein string anhand dessen versucht werden soll die passende Box zu finden</param>
        public bool TryGetShortName(string input, out string ShortName)
        {
            ShortName = null;
            string result = GetShortName(input);
            if (result != input)
            {
                ShortName = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Versucht anhand des Inputs mit den gegebenen Daten die passende Box zu ermitteln
        /// </summary>
        public IEnumerable<Box> FindBoxes(string input)
        {
            return GetBoxen().Where(x => x.Matches(input));
        }

        /// <summary>
        /// Versucht anhand des Inputs mit den gegebenen Daten die einzig passende Box zu ermitteln
        /// </summary>
        public bool TryFindExactBox(string input, out Box box)
        {
            box = null;
            try
            {
                box = FindBoxes(input).Single();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Box> GetBoxen()
        {
            return Boxen;
        }
    }

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
            return ((ShortName == input) || (FullName == input) || RegexPattern.Any(x => Regex.Match(input, x).Success));
        }

        /// <summary>
        /// Fügt einen Regulären Ausdruck zu den Erkennungsmustern hinzu. Doppelte Einträge werden automatisch entfernt
        /// </summary>
        public void AddRegex(params String[] pattern)
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
                return this.ShortName == ((Box)obj).ShortName;
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