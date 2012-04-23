using System;
using System.Collections.Generic;
using FritzBot;

namespace FritzBot.commands
{
    class remember : ICommand
    {
        public String[] Name { get { return new String[] { "remember" }; } }
        public String HelpText { get { return "Hinterlasse einem Benutzer eine Nachricht. Sobald er wiederkommt oder etwas schreibt werde ich sie ihm Zustellen. !remember <Benutzer> <Nachricht>"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {
            Program.UserJoined -= new Program.JoinEventHandler(SendJoin);
            Program.UserMessaged -= new Program.MessageEventHandler(SendMessaged);
        }

        public remember()
        {
            Program.UserJoined += new Program.JoinEventHandler(SendJoin);
            Program.UserMessaged += new Program.MessageEventHandler(SendMessaged);
        }

        private void SendJoin(Irc connection, String nick, String room)
        {
            List<String[]> AllUnread = Program.TheUsers[nick].GetAllRemembers();
            foreach (String[] OneUnread in AllUnread)
            {
                connection.Sendmsg(OneUnread[0] + " hat für dich am " + OneUnread[2].Replace("T", " um ") + " eine Nachricht hinterlassen: " + OneUnread[1], nick);
            }
        }

        public void SendMessaged(Irc connection, String sender, String receiver, String message)
        {
            SendJoin(connection, sender, receiver);
        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String[] split = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
            if (split.Length > 1)
            {
                if (Program.TheUsers.Exists(split[0]))
                {
                    Program.TheUsers[split[0]].AddRemember(sender, split[1]);
                }
            }
            else
            {
                connection.Sendmsg("Die Eingabe war nicht korrekt: !remember <Benutzer> <Nachricht>", receiver);
            }
        }
    }
}