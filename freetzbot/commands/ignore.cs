using System;

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

        public void Run(ircMessage theMessage)
        {
            if (theMessage.Source == theMessage.CommandLine || toolbox.IsOp(theMessage.Nick))
            {
                theMessage.TheUsers[theMessage.Nick].ignored = true;
                theMessage.Answer("Ich werde " + theMessage.CommandLine + " ab sofort keine beachtung mehr schenken");
            }
        }
    }
}