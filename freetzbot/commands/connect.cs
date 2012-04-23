using System;
using System.Net;
using FritzBot;

namespace FritzBot.commands
{
    class connect : ICommand
    {
        public String[] Name { get { return new String[] { "connect" }; } }
        public String HelpText { get { return "Baut eine Verbindung zu einem anderen IRC Server auf, Syntax: !connect server,port,nick,quit_message,initial_channel"; ; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String[] parameter = message.Split(new String[] { "," }, 5, StringSplitOptions.None);
            if (parameter.Length < 5)
            {
                connection.Sendmsg("Zu wenig Parameter! schau mal in die Hilfe", receiver);
            }
            if (parameter[2].Length > 9)
            {
                connection.Sendmsg("Hörmal, das RFC erlaubt nur Nicknames mit 9 Zeichen", receiver);
                return;
            }
            try
            {
                Convert.ToInt32(parameter[1]);
            }
            catch
            {
                connection.Sendmsg("Der PORT sollte eine gültige Ganzahl sein, Prüfe das", receiver);
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
                    connection.Sendmsg("Ich konnte die Adresse nicht auflösen, Prüfe nochmal ob deine Eingabe korrekt ist", receiver);
                    return;
                }
                toolbox.InstantiateConnection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
                toolbox.getDatabaseByName("servers.cfg").Add(message);
            }
            catch
            {
                connection.Sendmsg("Das hat nicht funktioniert, sorry", receiver);
            }
        }
    }
}