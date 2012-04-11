using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace freetzbot
{
    /// <summary>
    /// Eine Datenbank Klasse um Dateien zu speichern und auszulesen
    /// </summary>
    class db
    {
        public String datenbank_name;
        private List<String> datenbank;
        private static Boolean in_use;
        /// <summary>
        /// Die zu verwendende Datenbank
        /// </summary>
        /// <param name="db">Die Datenbank</param>
        public db(String db)
        {
            datenbank_name = db;
            in_use = false;
            Read();
        }
        /// <summary>
        /// Erstellt eine leere Datenbank Datei
        /// </summary>
        private void Initdb()
        {
            File.AppendAllText(datenbank_name, "");
        }
        /// <summary>
        /// Liest die Datenbank von einer Datei ein
        /// </summary>
        /// <returns>Gibt ein String Array mit dem Datenbank Inhalt zurück</returns>
        private void Read()
        {
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            if (!File.Exists(datenbank_name))
            {
                Initdb();
            }
            datenbank = new List<String>(File.ReadAllLines(datenbank_name, Encoding.GetEncoding("iso-8859-1")));
            Cleanup();
            in_use = false;
        }
        /// <summary>
        /// Schreibt die Datenbank in eine Datei
        /// </summary>
        public void Write()
        {
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            Cleanup();
            File.WriteAllLines(datenbank_name, datenbank.ToArray(), Encoding.GetEncoding("iso-8859-1"));
            in_use = false;
        }
        /// <summary>
        /// Fügt den angegebenen String am ende der Datenbank an
        /// </summary>
        /// <param name="to_add">Der String der in die Datenbank eingefügt werden soll</param>
        public void Add(String to_add)
        {
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            datenbank.Add(to_add);
            File.AppendAllText(datenbank_name, to_add + "\r\n", Encoding.GetEncoding("iso-8859-1"));
            in_use = false;
        }
        /// <summary>
        /// Entfernt den angegebenen String aus der Datenbank
        /// </summary>
        /// <param name="to_remove">Der String der gelöscht werden soll</param>
        /// <returns>Gibt false zurück wenn der String nicht gelöscht wurde oder true wenn er erfolgreich gelöscht wurde</returns>
        public Boolean Remove(String to_remove)
        {
            int index = Find(to_remove);
            if (index == -1)
            {
                return false;
            }
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            datenbank.RemoveAt(index);
            in_use = false;
            Write();
            return true;
        }
        /// <summary>
        /// Gibt die Stelle des angegebenen Strings in der Datenbank zurück
        /// </summary>
        /// <param name="to_find">Der String dessen Stelle in der DB gefunden werden soll</param>
        /// <returns>Einen Integer der die Position in der Datenbank representiert</returns>
        public int Find(String to_find)
        {
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            for (int i = 0; i < datenbank.Count; i++)
            {
                if (datenbank[i] == to_find)
                {
                    in_use = false;
                    return i;
                }
            }
            in_use = false;
            return -1;
        }
        /// <summary>
        /// Gibt alle Zeilen zurück die den angegebenen Suchstring beinhalten
        /// </summary>
        /// <param name="to_get">Der Suchstring den die Zeilen enthalten sollen</param>
        /// <returns></returns>
        public String[] GetContaining(String to_get)
        {
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            List<String> found = new List<String>();
            for (int i = 0; i < datenbank.Count; i++)
            {
                if (datenbank[i].ToLower().Contains(to_get.ToLower()))
                {
                    found.Add(datenbank[i]);
                }
            }
            in_use = false;
            return found.ToArray();
        }
        /// <summary>
        /// Gibt den String an der angegebenen Stelle zurück
        /// </summary>
        /// <param name="index">Die Stelle dessen Wert erwartet wird</param>
        /// <returns>Gibt den String an der angegebenen Stelle zurück</returns>
        public String GetAt(int index)
        {
            return datenbank[index];
        }
        /// <summary>
        /// Gibt ein String Array mit der gesamten Datenbank zurück
        /// </summary>
        /// <returns>Gibt ein String Array mit der gesamten Datenbank zurück</returns>
        public String[] GetAll()
        {
            return datenbank.ToArray();
        }
        /// <summary>
        /// Gibt einen Integer mit der Anzahl der Zeilen der DB zurück
        /// </summary>
        /// <returns>Gibt einen Integer mit der Anzahl der Zeilen der DB zurück</returns>
        public int Size()
        {
            return datenbank.Count;
        }
        /// <summary>
        /// Lädt die Datenbank aus der Datei neu in den Buffer
        /// </summary>
        public void Reload()
        {
            while (in_use)
            {
                Thread.Sleep(50);
            }
            in_use = true;
            datenbank = null;
            in_use = false;
            Read();
        }
        /// <summary>
        /// Löscht alle leeren Strings aus der Datenbank
        /// </summary>
        private void Cleanup()
        {
            for (int i = 0; i < datenbank.Count; i++)
            {
                if (datenbank[i] == "")
                {
                    datenbank.RemoveAt(i);
                }
            }
        }
    }
}