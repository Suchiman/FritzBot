using System;

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

        public void run(irc connection, String sender, String receiver, String message)
        {
            db seendb = toolbox.getDatabaseByName("seen.db");
            if (message.ToLower() == connection.nickname.ToLower())
            {
                connection.sendmsg("Ich bin gerade hier und was ich schreibe siehst du ja auch :-)", receiver);
                return;
            }
            if (seendb.GetContaining(message + ";").Length > 0)
            {
                String output = "";
                String[] daten = seendb.GetContaining(message + ";")[0].Split(';');//User;Joined;Messaged;Message
                DateTime seen;
                if (DateTime.TryParse(daten[1], out seen))
                {
                    output = "Den habe ich hier zuletzt am " + seen.ToString("dd.MM.yyyy ") + "um" + seen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                }
                if (daten[2] != "")
                {
                    DateTime said;
                    if (DateTime.TryParse(daten[2], out said))
                    {
                        if (output != "")
                        {
                            output += " ";
                        }
                        output += "Am " + said.ToString("dd.MM.yyyy ") + "um" + said.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + daten[3] + "\"";
                    }
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

        public static void joined(String nick)
        {
            if (!nick.Contains("."))
            {
                setdata(nick, 1, "", 0);
            }
        }

        public static void quit(String nick)
        {
            if (!nick.Contains("."))
            {
                setdata(nick, 1, "", 2);
            }
        }

        public static void messaged(String nick, String message)
        {
            if (!nick.Contains("."))
            {
                setdata(nick, 2, message);
            }
        }

        public static void setdata(String nick, int messaged = 1, String message = "", int joined = 0)
        {
            try
            {
                db seendb = toolbox.getDatabaseByName("seen.db");
                if (!(seendb.GetContaining(nick + ";").Length > 0))
                {
                    seendb.Add(nick + ";;;");
                }
                String data = seendb.GetContaining(nick + ";")[0];
                String datum = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                String[] split = data.Split(';'); //User;Joined;Messaged;Message
                String joindate = "";
                String messagedate = "";
                String messagetext = "";
                if (joined == 0)
                {
                    joindate = "";
                }
                if (joined == 1)
                {
                    joindate = split[1];
                }
                if (joined == 2)
                {
                    joindate = datum;
                }
                if (messaged == 0)
                {
                    messagedate = "";
                }
                if (messaged == 1)
                {
                    messagedate = split[2];
                }
                if (messaged == 2)
                {
                    messagedate = datum;
                }
                if (message != "")
                {
                    messagetext = message;
                }
                else
                {
                    messagetext = split[3];
                }
                seendb.Remove(data);
                seendb.Add(nick + ";" + joindate + ";" + messagedate + ";" + messagetext);
            }
            catch
            {

            }
        }
    }
}