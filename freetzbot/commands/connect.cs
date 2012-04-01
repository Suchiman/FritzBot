using System;
using System.Net;

namespace freetzbot.commands
{
    class connect : command
    {
        private String[] name = { "connect" };
        private String helptext = "Baut eine Verbindung zu einem anderen IRC Server auf, Syntax: !connect server,port,nick,quit_message,initial_channel";
        private Boolean op_needed = true;
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

        public void run(irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { "," }, 5, StringSplitOptions.None);
            if (parameter.Length < 5)
            {
                connection.sendmsg("Zu wenig Parameter! schau mal in die Hilfe", receiver);
            }
            if (parameter[2].Length > 9)
            {
                connection.sendmsg("Hörmal, das RFC erlaubt nur Nicknames mit 9 Zeichen", receiver);
                return;
            }
            try
            {
                Convert.ToInt32(parameter[1]);
            }
            catch
            {
                connection.sendmsg("Der PORT sollte eine gültige Ganzahl sein, Prüfe das", receiver);
                return;
            }
            try
            {
                try
                {
                    IPHostEntry hostInfo = Dns.GetHostEntry(parameter[0]);
                }
                catch
                {
                    connection.sendmsg("Ich konnte die Adresse nicht auflösen, Prüfe nochmal ob deine Eingabe korrekt ist", receiver);
                    return;
                }
                toolbox.instantiate_connection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                toolbox.getDatabaseByName("servers.cfg").Add(message);
            }
            catch
            {
                connection.sendmsg("Das hat nicht funktioniert, sorry", receiver);
            }
        }
    }
}