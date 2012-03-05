using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace freetzbot
{
    class settings
    {
        private String settingsfile;
        private Mutex threadsafe;
        private List<String> settingslist = new List<String>();

        public settings(String file)
        {
            settingsfile = file;
            read_settings();
            threadsafe = new Mutex();
        }

        private void read_settings()
        {
            if (!File.Exists(settingsfile))
            {
                StreamWriter sstream = new StreamWriter(settingsfile, false, Encoding.GetEncoding("iso-8859-1"));
                sstream.Write("");
                sstream.Close();
            }
            StreamReader stream = new StreamReader(settingsfile, Encoding.GetEncoding("iso-8859-1"));
            while (stream.Peek() >= 0)
            {
                settingslist.Add(stream.ReadLine());
            }
            stream.Close();
        }

        public String get(String option)
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

        public void set(String option, String to_set)
        {
            threadsafe.WaitOne();
            Boolean exist = false;
            for (int i = 0; settingslist.Count > i; i++)
            {
                if (settingslist[i].Contains(option + "="))
                {
                    settingslist[i] = option + "=" + to_set;
                    exist = true;
                }
            }
            if (exist == false)
            {
                settingslist.Add(option + "=" + to_set);
            }
            StreamWriter stream = new StreamWriter(settingsfile, false, Encoding.GetEncoding("iso-8859-1"));
            foreach (String data in settingslist)
            {
                stream.WriteLine(data);
            }
            stream.Close();
            threadsafe.ReleaseMutex();
        }
    }
}