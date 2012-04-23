using System;
using FritzBot;

namespace FritzBot.commands
{
    class restart : ICommand
    {
        public String[] Name { get { return new String[] { "restart" }; } }
        public String HelpText { get { return "Ich werde versuchen mich selbst neuzustarten, Operator Befehl: kein parameter"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            Program.restart = true;
            Program.Trennen();
        }
        
    }
}