using System;
using FritzBot;

namespace FritzBot.commands
{
    class boxlist : ICommand
    {
        public String[] Name { get { return new String[] { "boxlist" }; } }
        public String HelpText { get { return "Dies listet alle registrierten Boxtypen auf."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String boxen = "";
            foreach (User oneuser in Program.TheUsers)
            {
                foreach (String box in oneuser.boxes)
                {
                    if (!boxen.Contains(box))
                    {
                        boxen += ", " + box;
                    }
                }
            }
            boxen = boxen.Remove(0, 2);
            connection.Sendmsg("Folgende Boxen wurden bei mir registriert: " + boxen, receiver);
        }
    }
}