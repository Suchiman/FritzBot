using System;
using System.Text;

namespace FritzBot.commands
{
    class google : ICommand
    {
        public String[] Name { get { return new String[] { "google", "g" }; } }
        public String HelpText { get { return "Syntax: (!g) !google etwas das du suchen möchtest"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String output = "https://www.google.de/search?q=";
            if (String.IsNullOrEmpty(message))
            {
                output = "http://www.google.de/";
            }
            else
            {
                output += System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes("\"" + message + "\""));
            }
            connection.Sendmsg(output, receiver);
        }
    }
}