using FritzBot.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FritzBot.DataModel
{
    public class BoxDatabase : PluginBase
    {
        private static BoxDatabase instance;
        private List<Box> Boxen;

        public static BoxDatabase GetInstance()
        {
            Contract.Ensures(Contract.Result<BoxDatabase>() != null);

            if (instance == null)
            {
                instance = new BoxDatabase();
            }
            return instance;
        }

        public BoxDatabase()
        {
            try
            {
                using (var context = new BotContext())
                {
                    Boxen = context.Boxes.Include(x => x.RegexPattern).ToList();
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging("Fehler beim Laden der BoxDatabase");
                toolbox.Logging(ex);
            }
        }

        /// <summary>
        /// Fügt eine neue Box hinzu
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box, z.B. 7270v2</param>
        /// <param name="FullName">Der vollständige Name</param>
        /// <param name="RegexPattern">Ein oder mehrere Reguläre Ausdrücke um die Box zu erkennen</param>
        public Box AddBox(string ShortName, string FullName, params string[] RegexPattern)
        {
            Contract.Ensures(Contract.Result<Box>() != null);

            if (GetBoxByShortName(ShortName) != null)
            {
                throw new Exception("Es gibt bereits eine Box mit gleichem ShortName");
            }
            Box box = new Box()
            {
                ShortName = ShortName,
                FullName = FullName,
                RegexPattern = RegexPattern.Select(x => new BoxRegexPattern { Pattern = x }).ToList()
            };
            using (var context = new BotContext())
            {
                context.Boxes.Add(box);
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
}