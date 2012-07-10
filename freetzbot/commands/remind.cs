using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    class remind : ICommand
    {
        public String[] Name { get { return new String[] { "remind" }; } }
        public String HelpText { get { return "Hinterlasse einem Benutzer eine Nachricht. Sobald er wiederkommt oder etwas schreibt werde ich sie ihm Zustellen. !remind <Benutzer> <Nachricht>"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {
            Program.UserJoined -= new Program.JoinEventHandler(SendJoin);
            Program.UserMessaged -= new Program.MessageEventHandler(SendMessaged);
        }

        public remind()
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

        public void SendMessaged(ircMessage theMessage)
        {
            List<String[]> AllUnread = theMessage.TheUser.GetAllRemembers();
            foreach (String[] OneUnread in AllUnread)
            {
                theMessage.SendPrivateMessage(OneUnread[0] + " hat für dich am " + OneUnread[2].Replace("T", " um ") + " eine Nachricht hinterlassen: " + OneUnread[1]);
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count > 1)
            {
                if (theMessage.TheUsers.Exists(theMessage.CommandArgs[0]))
                {
                    theMessage.TheUsers[theMessage.CommandArgs[0]].AddRemember(theMessage.Nick, theMessage.CommandLine.Substring(theMessage.CommandLine.IndexOf(' ')));
                    theMessage.Answer("Okay ich werde es sobald wie möglich zustellen");
                }
                else
                {
                    theMessage.Answer("Den Benutzer habe ich aber noch nie gesehen");
                }
            }
            else
            {
                theMessage.Answer("Die Eingabe war nicht korrekt: !remember <Benutzer> <Nachricht>");
            }
        }
    }
}