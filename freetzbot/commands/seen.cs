using System;

namespace freetzbot.commands
{
    class seen : command
    {
        private String[] name = { "seen" };
        private String helptext = "Gibt aus wann der Nutzer zuletzt gesehen wurde.";
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
            if (toolbox.getDatabaseByName("user.db").GetContaining(message).Length > 0)
            {
                if (toolbox.getDatabaseByName("user.db").GetContaining(message)[0].Contains(","))
                {
                    String datum = toolbox.getDatabaseByName("user.db").GetContaining(message)[0].Split(',')[1];
                    DateTime seen;
                    DateTime.TryParse(datum, out seen);
                    connection.sendmsg("Den habe ich hier zuletzt am " + seen.ToString("dd.MM.yyyy ") + "um" + seen.ToString(" HH:mm:ss ") + "Uhr gesehen", receiver);
                }
                else
                {
                    connection.sendmsg("Der ist doch gerade hier ;)", receiver);
                }
            }
            else
            {
                connection.sendmsg("Diesen Benutzer habe ich noch nie gesehen", receiver);
            }
        }
    }
}