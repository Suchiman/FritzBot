using System;
using FritzBot;

namespace FritzBot.commands
{
    class title : ICommand
    {
        public String[] Name { get { return new String[] { "title" }; } }
        public String HelpText { get { return "Gibt den Titel der Seite wieder"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            try
            {
                String webpage = toolbox.GetWeb(message);
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
                connection.Sendmsg(title, receiver);
            }
            catch
            {
                connection.Sendmsg("Entweder hat die Webseite keine Überschrift oder die URL ist nicht gültig", receiver);
            }
        }
    }
}
