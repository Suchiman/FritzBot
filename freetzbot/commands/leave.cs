using System;

namespace FritzBot.commands
{
    class leave : ICommand
    {
        public String[] Name { get { return new String[] { "leave" }; } }
        public String HelpText { get { return "Zum angegebenen Server werde ich die Verbindung trennen, Operator Befehl: !leave test.de"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            Program.TheServers[theMessage.CommandLine] = null;
        }
    }
}