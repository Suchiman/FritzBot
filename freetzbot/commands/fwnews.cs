using System;
using System.Collections.Generic;
using System.Text;

namespace FritzBot.commands
{
    class fwnews :ICommand
    {
        public String[] Name { get { return new String[] { "fwnews" }; } }
        public String HelpText { get { return "Erstattet automatisch bericht, wenn eine neue Firmware auf dem FTP rauskommt"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            theMessage.Answer("lol");
        }
    }
}