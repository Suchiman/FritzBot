/*using System;
using System.Collections.Generic;
using System.Text;

namespace FritzBot.commands
{
    class fwnews : ICommand
    {
        public String[] Name { get { return new String[] { "fwnews" }; } }
        public String HelpText { get { return "Erstattet automatisch bericht, wenn eine neue Firmware auf dem FTP rauskommt"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public fwnews()
        {

        }

        public void Run(ircMessage theMessage)
        {
            List<String> content = GetAll("ftp://ftp.avm.de/fritz.box/fritzbox.fon_wlan_7270_v2/");
            foreach (String cont in content)
            {
                theMessage.Answer(cont);
            }
        }

        private List<String> Date = new List<String>();
        private List<String> Address = new List<String>();

        private List<String> GetAll(String Address)
        {
            String BoxdirectoryContent = fw.FtpDirectory(Address);
            String[] boxes = BoxdirectoryContent.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<String> found = new List<String>();
            foreach (String daten in boxes)
            {
                if (daten.ToCharArray()[0] == 'd')
                {
                    String pfad = daten.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    found.AddRange(GetAll(Address + pfad + "/"));
                }
                else
                {
                    String file = daten.Split(new String[] { " " }, 9, StringSplitOptions.RemoveEmptyEntries)[8];
                    found.Add(Address + file);
                }
            }
            return found;
        }
    }
}*/