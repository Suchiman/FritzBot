using System;
using System.Threading;

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

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (message.ToLower() == connection.Nickname.ToLower())
            {
                connection.Sendmsg("Ich bin gerade hier und was ich schreibe siehst du ja auch :-)", receiver);
                return;
            }
            if (FritzBot.Program.TheUsers.Exists(message))
            {
                String output = "";

                FritzBot.Program.await_response = true;
                connection.Sendraw("NAMES");
                while (FritzBot.Program.await_response)
                {
                    Thread.Sleep(50);
                }
                String response = FritzBot.Program.awaited_response;
                if (response.Contains(message))
                {
                    FritzBot.Program.TheUsers[message].last_seen = DateTime.MinValue;
                }
                if (FritzBot.Program.TheUsers[message].last_seen != DateTime.MinValue)
                {
                    output = "Den/Die habe ich hier zuletzt am " + FritzBot.Program.TheUsers[message].last_seen.ToString("dd.MM.yyyy ") + "um" + FritzBot.Program.TheUsers[message].last_seen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                }
                if (FritzBot.Program.TheUsers[message].last_messaged != DateTime.MinValue)
                {
                    if (!String.IsNullOrEmpty(output))
                    {
                        output += " ";
                    }
                    output += "Am " + FritzBot.Program.TheUsers[message].last_messaged.ToString("dd.MM.yyyy ") + "um" + FritzBot.Program.TheUsers[message].last_messaged.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + FritzBot.Program.TheUsers[message].last_message + "\"";
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