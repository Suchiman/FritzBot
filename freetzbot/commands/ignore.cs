using System;
using FritzBot;

namespace FritzBot.commands
{
    class ignore : ICommand
    {
        public String[] Name { get { return new String[] { "ignore" }; } }
        public String HelpText { get { return "Schließt die angegebene Person von mir aus"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (sender == message || toolbox.IsOp(sender))
            {
                Program.TheUsers[message].ignored = true;
                connection.Sendmsg("Ich werde " + message + " ab sofort keine beachtung mehr schenken", receiver);
            }
        }
    }
}