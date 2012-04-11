using System;
using System.Threading;

namespace freetzbot.commands
{
    class seen : command
    {
        private String[] name = { "seen", "said" };
        private String helptext = "Gibt aus wann der Nutzer zuletzt gesehen wurde und wann er was zuletzt sagte.";
        private Boolean op_needed = false;
        private Boolean parameter_needed = true;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            if (message.ToLower() == connection.nickname.ToLower())
            {
                connection.sendmsg("Ich bin gerade hier und was ich schreibe siehst du ja auch :-)", receiver);
                return;
            }
            if (freetzbot.Program.TheUsers.Exists(message))
            {
                String output = "";

                freetzbot.Program.await_response = true;
                connection.sendraw("NAMES");
                while (freetzbot.Program.await_response)
                {
                    Thread.Sleep(50);
                }
                String response = freetzbot.Program.awaited_response;
                if (response.Contains(message))
                {
                    freetzbot.Program.TheUsers[message].last_seen = DateTime.MinValue;
                }
                if (freetzbot.Program.TheUsers[message].last_seen != DateTime.MinValue)
                {
                    output = "Den/Die habe ich hier zuletzt am " + freetzbot.Program.TheUsers[message].last_seen.ToString("dd.MM.yyyy ") + "um" + freetzbot.Program.TheUsers[message].last_seen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                }
                if (freetzbot.Program.TheUsers[message].last_messaged != DateTime.MinValue)
                {
                    if (output != "")
                    {
                        output += " ";
                    }
                    output += "Am " + freetzbot.Program.TheUsers[message].last_messaged.ToString("dd.MM.yyyy ") + "um" + freetzbot.Program.TheUsers[message].last_messaged.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + freetzbot.Program.TheUsers[message].last_message + "\"";
                }
                if (output != "")
                {
                    connection.sendmsg(output, receiver);
                }
                else
                {
                    connection.sendmsg("Scheinbar sind meine Datensätze unvollständig, tut mir leid", receiver);
                }
            }
            else
            {
                connection.sendmsg("Diesen Benutzer habe ich noch nie gesehen", receiver);
            }
        }
    }
}