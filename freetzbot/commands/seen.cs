using System;
using System.Threading;
using FritzBot;

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
            Program.TheUsers[nick].authenticated = false;
        }

        private void nick(Irc connection, String Oldnick, String Newnick)
        {
            if (!Program.TheUsers.Exists(Oldnick))
            {
                Program.TheUsers[Oldnick].AddName(Newnick);
                Program.TheUsers[Oldnick].authenticated = false;
            }
        }

        private void message(Irc connection, String source, String nick, String message)
        {
            if (!nick.Contains(".") && nick != connection.Nickname)
            {
                Program.TheUsers[nick].SetMessage(message);
            }
        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (message.ToLower() == connection.Nickname.ToLower())
            {
                connection.Sendmsg("Ich bin gerade hier und was ich schreibe siehst du ja auch :-)", receiver);
                return;
            }
            if (Program.TheUsers.Exists(message))
            {
                String output = "";

                Program.await_response = true;
                connection.Sendraw("NAMES");
                while (Program.await_response)
                {
                    Thread.Sleep(50);
                }
                String response = Program.awaited_response;
                if (response.Contains(message))
                {
                    Program.TheUsers[message].last_seen = DateTime.MinValue;
                }
                if (Program.TheUsers[message].last_seen != DateTime.MinValue)
                {
                    output = "Den/Die habe ich hier zuletzt am " + Program.TheUsers[message].last_seen.ToString("dd.MM.yyyy ") + "um" + Program.TheUsers[message].last_seen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                }
                if (Program.TheUsers[message].last_messaged != DateTime.MinValue)
                {
                    if (!String.IsNullOrEmpty(output))
                    {
                        output += " ";
                    }
                    output += "Am " + Program.TheUsers[message].last_messaged.ToString("dd.MM.yyyy ") + "um" + Program.TheUsers[message].last_messaged.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + Program.TheUsers[message].last_message + "\"";
                }
                if (!String.IsNullOrEmpty(output))
                {
                    connection.Sendmsg(output, receiver);
                }
                else
                {
                    connection.Sendmsg("Scheinbar sind meine Datensätze unvollständig, tut mir leid", receiver);
                }
            }
            else
            {
                connection.Sendmsg("Diesen Benutzer habe ich noch nie gesehen", receiver);
            }
        }
    }
}