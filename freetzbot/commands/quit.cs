﻿using System;

namespace FritzBot.commands
{
    class quit : ICommand
    {
        public String[] Name { get { return new String[] { "quit" }; } }
        public String HelpText { get { return "Das beendet mich X_x"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            Program.TheServers.DisconnectAll();
        }
    }
}