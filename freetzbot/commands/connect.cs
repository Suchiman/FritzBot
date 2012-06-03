using System;
using System.Net;

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

        public void Run(ircMessage theMessage)
        {
            String[] parameter = theMessage.CommandLine.Split(new String[] { "," }, 5, StringSplitOptions.None);
            if (parameter.Length < 5)
            {
                theMessage.Answer("Zu wenig Parameter! schau mal in die Hilfe");
            }
            if (parameter[2].Length > 9)
            {
                theMessage.Answer("Hörmal, das RFC erlaubt nur Nicknames mit 9 Zeichen");
                return;
            }
            try
            {
                int port = Convert.ToInt32(parameter[1]);
                if (!(port > 0 && port < 65536))
                {
                    theMessage.Answer("Gültige Ports liegen zwischen 0 und 65536");
                    return;
                }
            }
            catch
            {
                theMessage.Answer("Der PORT sollte eine gültige Ganzahl sein, Prüfe das");
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
                    theMessage.Answer("Ich konnte die Adresse nicht auflösen, Prüfe nochmal ob deine Eingabe korrekt ist");
                    return;
                }
                toolbox.InstantiateConnection(parameter[0], Convert.ToInt32(parameter[1]), parameter[2], parameter[3], parameter[4]);
            }
            catch
            {
                theMessage.Answer("Das hat nicht funktioniert, sorry");
            }
        }
    }
}