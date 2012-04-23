using System;
using System.Text;
using FritzBot;

namespace FritzBot.commands
{
    class lmgtfy : ICommand
    {
        public String[] Name { get { return new String[] { "lmgtfy" }; } }
        public String HelpText { get { return "Die Funktion benötigt einen Parameter!"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            connection.Sendmsg("http://lmgtfy.com/?q=" + System.Web.HttpUtility.UrlEncode(Encoding.GetEncoding("iso-8859-1").GetBytes(message)), receiver);
        }
    }
}