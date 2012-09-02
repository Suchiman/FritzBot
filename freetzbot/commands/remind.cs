using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("remind")]
    [Module.Help("Hinterlasse einem Benutzer eine Nachricht. Sobald er wiederkommt oder etwas schreibt werde ich sie ihm Zustellen. !remind <Benutzer> <Nachricht>")]
    class remind : ICommand, IBackgroundTask
    {
        public void Stop()
        {
            Program.UserJoined -= new Program.JoinEventHandler(SendJoin);
            Program.UserMessaged -= new Program.MessageEventHandler(SendMessaged);
        }

        public void Start()
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
                theMessage.Answer("Die Eingabe war nicht korrekt: !remind <Benutzer> <Nachricht>");
            }
        }
    }
}