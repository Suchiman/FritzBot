using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace FritzBot
{
    class settings
    {
        private String settingsfile;
        private Mutex threadsafe;
        private List<String> settingslist = new List<String>();

        public settings(String file)
        {
            settingsfile = file;
            ReadSettings();
            threadsafe = new Mutex();
        }

        private void ReadSettings()
        {
            if (!File.Exists(settingsfile))
            {
                File.AppendAllText(settingsfile, "");
            }
            settingslist = new List<String>(File.ReadAllLines(settingsfile, Encoding.GetEncoding("iso-8859-1")));
        }

        public String this[String option]
        {
            get
            {
                threadsafe.WaitOne();
                foreach (String data in settingslist)
                {
                    String[] splitted = data.Split(new String[] { "=" }, 2, StringSplitOptions.None);
                    if (splitted[0] == option)
                    {
                        threadsafe.ReleaseMutex();
                        return splitted[1];
                    }
                }
                threadsafe.ReleaseMutex();
                return "";
            }
            set
            {
                threadsafe.WaitOne();
                Boolean exist = false;
                for (int i = 0; settingslist.Count > i; i++)
                {
                    if (settingslist[i].Contains(option + "="))
                    {
                        settingslist[i] = option + "=" + value;
                        exist = true;
                    }
                }
                if (exist == false)
                {
                    settingslist.Add(option + "=" + value);
                }
                for (int i = 0; i < settingslist.Count; i++)
                {
                    if (String.IsNullOrEmpty(settingslist[i]))
                    {
                        settingslist.RemoveAt(i);
                    }
                }
                File.WriteAllLines(settingsfile, settingslist.ToArray(), Encoding.GetEncoding("iso-8859-1"));
                threadsafe.ReleaseMutex();
            }
        }
    }
}