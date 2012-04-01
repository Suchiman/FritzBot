using System;

namespace freetzbot.commands
{
    class boxinfo : command
    {
        private String[] name = { "boxinfo" };
        private String helptext = "Zeigt die Box/en des angegebenen Benutzers an.";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
        private Boolean accept_every_param = true;

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
            String[] Daten = toolbox.getDatabaseByName("box.db").GetContaining(message);
            if (Daten.Length > 0)
            {
                String boxen = "";
                for (int i = 0; i < Daten.Length; i++)
                {
                    String[] user = Daten[i].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                    if (boxen != "")
                    {
                        boxen += ", " + user[1];
                    }
                    else
                    {
                        boxen = user[1];
                    }
                }
                if (message == sender)
                {
                    connection.sendmsg("Du hast bei mir die Box/en " + boxen + " registriert.", receiver);
                }
                else
                {
                    connection.sendmsg(message + " sagte mir er hätte die Box/en " + boxen, receiver);
                }
            }
            else
            {
                if (message == sender)
                {
                    connection.sendmsg("Du hast bei mir noch keine Box registriert.", receiver);
                }
                else
                {
                    connection.sendmsg("Über den habe ich keine Informationen.", receiver);
                }
            }
        }
    }
}