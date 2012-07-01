using System;

namespace FritzBot.commands
{
    class seen : ICommand
    {
        public String[] Name { get { return new String[] { "seen", "said" }; } }
        public String HelpText { get { return "Gibt aus wann der Nutzer zuletzt gesehen wurde und wann er was zuletzt sagte."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {
            Program.UserJoined -= new Program.JoinEventHandler(joined);
            Program.UserQuit -= new Program.QuitEventHandler(quit);
            Program.UserPart -= new Program.PartEventHandler(part);
            Program.UserNickChanged -= new Program.NickEventHandler(nick);
            Program.UserMessaged -= new Program.MessageEventHandler(message);
        }

        public seen()
        {
            Program.UserJoined += new Program.JoinEventHandler(joined);
            Program.UserQuit += new Program.QuitEventHandler(quit);
            Program.UserPart += new Program.PartEventHandler(part);
            Program.UserNickChanged += new Program.NickEventHandler(nick);
            Program.UserMessaged += new Program.MessageEventHandler(message);
        }

        private void joined(Irc connection, String nick, String Room)
        {
            Program.TheUsers[nick].last_seen = DateTime.MinValue;
        }

        private void part(Irc connection, String nick, String Room)
        {
            quit(connection, nick);
        }

        private void quit(Irc connection, String nick)
        {
            Program.TheUsers[nick].SetSeen();
            Program.TheUsers[nick].Authenticated = false;
        }

        private void nick(Irc connection, String Oldnick, String Newnick)
        {
            if (!Program.TheUsers.Exists(Oldnick))
            {
                Program.TheUsers[Oldnick].AddName(Newnick);
                Program.TheUsers[Oldnick].Authenticated = false;
            }
        }

        private void message(ircMessage theMessage)
        {
            if (!theMessage.Nick.Contains(".") && theMessage.Nick != theMessage.Connection.Nickname)
            {
                theMessage.TheUser.SetMessage(theMessage.Message);
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandLine.ToLower() == theMessage.Connection.Nickname.ToLower())
            {
                theMessage.Answer("Ich bin gerade hier und was ich schreibe siehst du ja auch :-)");
                return;
            }
            if (theMessage.TheUsers.Exists(theMessage.CommandLine))
            {
                String output = "";
                if (theMessage.TheUsers[theMessage.CommandLine].last_seen != DateTime.MinValue)
                {
                    output = "Den/Die habe ich hier zuletzt am " + theMessage.TheUsers[theMessage.CommandLine].last_seen.ToString("dd.MM.yyyy ") + "um" + theMessage.TheUsers[theMessage.CommandLine].last_seen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                }
                if (theMessage.TheUsers[theMessage.CommandLine].last_messaged != DateTime.MinValue)
                {
                    if (!String.IsNullOrEmpty(output))
                    {
                        output += " ";
                    }
                    output += "Am " + theMessage.TheUsers[theMessage.CommandLine].last_messaged.ToString("dd.MM.yyyy ") + "um" + theMessage.TheUsers[theMessage.CommandLine].last_messaged.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + theMessage.TheUsers[theMessage.CommandLine].last_message + "\"";
                }
                if (!String.IsNullOrEmpty(output))
                {
                    theMessage.Answer(output);
                }
                else
                {
                    theMessage.Answer("Scheinbar sind meine Datensätze unvollständig, tut mir leid");
                }
            }
            else
            {
                theMessage.Answer("Diesen Benutzer habe ich noch nie gesehen");
            }
        }
    }
}