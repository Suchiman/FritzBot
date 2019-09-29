using FritzBot.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FritzBot.DataModel
{
    public static class BoxDatabase
    {
        private static readonly List<Box> _boxen;

        public static IEnumerable<Box> Boxen => _boxen;

        static BoxDatabase()
        {
            try
            {
                using var context = new BotContext();
                _boxen = context.Boxes.Include(x => x.RegexPattern).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fehler beim Laden der BoxDatabase");
                throw;
            }
        }

        /// <summary>
        /// Fügt eine neue Box hinzu
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box, z.B. 7270v2</param>
        /// <param name="FullName">Der vollständige Name</param>
        /// <param name="RegexPattern">Ein oder mehrere Reguläre Ausdrücke um die Box zu erkennen</param>
        public static Box AddBox(string ShortName, string FullName, params string[] RegexPattern)
        {
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
            _boxen.Add(box);
            return box;
        }

        /// <summary>
        /// Gibt die Box mit entsprechendem ShortName zurück
        /// </summary>
        /// <param name="ShortName">Der Kurzname der Box</param>
        public static Box GetBoxByShortName(string ShortName)
        {
            return _boxen.FirstOrDefault(x => x.ShortName == ShortName);
        }

        /// <summary>
        /// Versucht die zum input passende Box zu finden und den ShortName zurückzugeben. Andernfalls ist die Rückgabe der input
        /// </summary>
        /// <param name="input">Ein string anhand dessen versucht werden soll die passende Box zu finden</param>
        public static string GetShortName(string input)
        {
            if (TryFindExactBox(input, out Box? ToFind))
            {
                return ToFind.ShortName;
            }
            return input;
        }

        /// <summary>
        /// Versucht die zum input passende Box zu finden und den ShortName zurückzugeben.
        /// </summary>
        /// <param name="input">Ein string anhand dessen versucht werden soll die passende Box zu finden</param>
        public static bool TryGetShortName(string input, [NotNullWhen(true)]out string? ShortName)
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
        public static IEnumerable<Box> FindBoxes(string input)
        {
            return _boxen.Where(x => x.Matches(input));
        }

        /// <summary>
        /// Versucht anhand des Inputs mit den gegebenen Daten die einzig passende Box zu ermitteln
        /// </summary>
        public static bool TryFindExactBox(string input, [NotNullWhen(true)]out Box? box)
        {
            box = null;
            var boxes = FindBoxes(input).Take(2).ToArray();
            if (boxes.Length != 1)
            {
                return false;
            }
            box = boxes[0];
            return true;
        }
    }
}