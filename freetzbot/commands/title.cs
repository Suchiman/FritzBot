using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.commands
{
    class title : command
    {
        private String[] name = { "title" };
        private String helptext = "Gibt den Titel der Seite wieder";
        private Boolean op_needed = false;
        private Boolean parameter_needed = true;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            try
            {
                String webpage = toolbox.get_web(message);
                String title = webpage.Split(new String[] { "<title>" }, 8, StringSplitOptions.None)[1].Split(new String[] { "</title>" }, 2, StringSplitOptions.None)[0];
                while (title.IndexOf('\n') != -1)
                {
                    title = title.Remove(title.IndexOf('\n'), 1);
                }
                while (title.Contains("  "))
                {
                    title = title.Replace("  ", " ");
                }
                if(title.ToCharArray()[0] == ' ')
                {
                    title = title.Remove(0, 1);
                }
                if (title.ToCharArray()[title.ToCharArray().Length - 1] == ' ')
                {
                    title = title.Remove(title.ToCharArray().Length - 1, 1);
                }
                connection.sendmsg(title, receiver);
            }
            catch
            {
                connection.sendmsg("Entweder hat die Webseite keine Überschrift oder die URL ist nicht gültig", receiver);
            }
        }
    }
}
