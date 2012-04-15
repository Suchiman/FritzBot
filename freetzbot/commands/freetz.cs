using System;
using System.Text;

namespace FritzBot.commands
{
    class freetz : ICommand
    {
        public String[] Name { get { return new String[] { "freetz", "f" }; } }
        public String HelpText { get { return "Das erzeugt einen Link zum Freetz Trac mit dem angegebenen Suchkriterium, Beispiele: !freetz ngIRCd, !freetz Build System"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String output = "http://freetz.org/search?q=";
            if (String.IsNullOrEmpty(message))
            {
                output = "http://freetz.org/wiki";
            }
            else
            {
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(message)) + "&wiki=on";
            }
            output = output.Replace("%23", "#");
            connection.Sendmsg(output, receiver);
        }
    }
}