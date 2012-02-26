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
        private String datenbank_name;
        private List<String> datenbank;
        private static Mutex threadsafe;
        /// <summary>
        /// Die zu verwendende Datenbank
        /// </summary>
        /// <param name="db">Die Datenbank</param>
        public db(String db)
        {
            threadsafe = new Mutex();
            datenbank_name = db;
            if (!File.Exists(datenbank_name))
            {
                Initdb();
            }
            datenbank = new List<String>(Read());
        }
        /// <summary>
        /// Erstellt eine leere Datenbank Datei
        /// </summary>
        private void Initdb()
        {
            threadsafe.WaitOne();
            StreamWriter db = new StreamWriter(datenbank_name, true, Encoding.GetEncoding("iso-8859-1"));
            db.WriteLine("");
            db.Close();
            threadsafe.ReleaseMutex();
        }
        /// <summary>
        /// Liest die Datenbank von einer Datei ein
        /// </summary>
        /// <returns>Gibt ein String Array mit dem Datenbank Inhalt zurück</returns>
        private String[] Read()
        {
            threadsafe.WaitOne();
            String[] Daten = new String[0];
            StreamReader db = new StreamReader(datenbank_name, Encoding.GetEncoding("iso-8859-1"));
            for (int i = 0; db.Peek() >= 0; i++)
            {
                String TempRead = db.ReadLine();
                if (TempRead.Length > 0)
                {
                    Array.Resize(ref Daten, Daten.Length + 1);
                    Daten[i] = TempRead;
                }
            }
            db.Close();
            threadsafe.ReleaseMutex();
            return Daten;
        }
        /// <summary>
        /// Schreibt die Datenbank in eine Datei
        /// </summary>
        private void Write()
        {
            threadsafe.WaitOne();
            String[] Daten = datenbank.ToArray();
            StreamWriter db = new StreamWriter(datenbank_name, false, Encoding.GetEncoding("iso-8859-1"));
            for (int i = 0; i < Daten.Length; i++)
            {
                db.WriteLine(Daten[i]);
            }
            db.Close();
            threadsafe.ReleaseMutex();
        }
        /// <summary>
        /// Fügt den angegebenen String am ende der Datenbank an
        /// </summary>
        /// <param name="to_add">Der String der in die Datenbank eingefügt werden soll</param>
        public void Add(String to_add)
        {
            threadsafe.WaitOne();
            datenbank.Add(to_add);
            threadsafe.ReleaseMutex();
            Write();
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
            threadsafe.WaitOne();
            datenbank.RemoveAt(index);
            threadsafe.ReleaseMutex();
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
            for (int i = 0; i < datenbank.Count; i++)
            {
                if (datenbank[i] == to_find)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Gibt alle Zeilen zurück die den angegebenen Suchstring beinhalten
        /// </summary>
        /// <param name="to_get">Der Suchstring den die Zeilen enthalten sollen</param>
        /// <returns></returns>
        public String[] GetContaining(String to_get)
        {
            List<String> found = new List<String>();
            for (int i = 0; i < datenbank.Count; i++)
            {
                if (datenbank[i].ToLower().Contains(to_get.ToLower()))
                {
                    found.Add(datenbank[i]);
                }
            }
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
            threadsafe.WaitOne();
            datenbank.Clear();
            threadsafe.ReleaseMutex();
            datenbank = new List<String>(Read());
        }
    }
}