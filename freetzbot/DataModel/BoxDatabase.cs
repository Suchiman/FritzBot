using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FritzBot.Core;

namespace FritzBot.DataModel
{
    public class BoxDatabase : PluginBase
    {
        private static BoxDatabase instance;

        public static BoxDatabase GetInstance()
        {
            if (instance == null)
            {
                instance = new BoxDatabase();
            }
            return instance;
        }

        /// <summary>
        /// Fügt eine neue Box hinzu
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box, z.B. 7270v2</param>
        /// <param name="FullName">Der vollständige Name</param>
        /// <param name="RegexPattern">Ein oder mehrere Reguläre Ausdrücke um die Box zu erkennen</param>
        public Box AddBox(string ShortName, string FullName, params String[] RegexPattern)
        {
            if (GetBoxByShortName(ShortName) != null)
            {
                throw new Exception("Es gibt bereits eine Box mit gleichem ShortName");
            }
            XElement NewBox = PluginStorage.GetNewElement("Box");
            NewBox.Add(new XElement("ShortName", ShortName), new XElement("FullName", FullName));
            XElement regexes = NewBox.AddSingle(new XElement("Regex"));
            foreach (string pattern in RegexPattern)
            {
                regexes.Add(new XElement("Pattern", pattern));
            }
            return new Box(NewBox);
        }

        /// <summary>
        /// Gibt die Box mit entsprechendem ShortName zurück
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box</param>
        public Box GetBoxByShortName(string ShortName)
        {
            XElement box = PluginStorage.Storage.Elements("Box").FirstOrDefault(x => x.Element("ShortName").Value == ShortName);
            return box != null ? new Box(box) : null;
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
            return PluginStorage.Storage.Elements("Box").Select(x => new Box(x));
        }
    }

    public class Box
    {
        private XElement storage;
        public string ShortName
        {
            get
            {
                return storage.Element("ShortName").Value;
            }
            set
            {
                storage.Element("ShortName").Value = value;
            }
        }
        public string FullName
        {
            get
            {
                return storage.Element("FullName").Value;
            }
            set
            {
                storage.Element("FullName").Value = value;
            }
        }
        public IEnumerable<string> RegexPattern
        {
            get
            {
                return storage.Element("Regex").Elements("Pattern").Select(x => x.Value);
            }
        }

        public Box(XElement box)
        {
            storage = box;
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
            pattern.ForEach(x => storage.Element("Regex").Add(new XElement("Pattern", x)));
            XElement[] patterns = storage.Element("Regex").Elements("Pattern").Distinct(x => x.Value).OrderBy(x => x.Value).ToArray();
            storage.Element("Regex").Elements().Remove();
            storage.Element("Regex").Add(patterns);
        }

        /// <summary>
        /// Entfernt einen Regulären Ausdruck aus den Erkennungsmustern
        /// </summary>
        public void RemoveRegex(string pattern)
        {
            storage.Element("Regex").Elements("Pattern").Where(x => x.Value == pattern).Remove();
        }

        /// <summary>
        /// Entfernt die Box
        /// </summary>
        public void Remove()
        {
            storage.Remove();
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