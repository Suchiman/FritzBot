using System;

namespace freetzbot.commands
{
    class boxremove : command
    {
        private String[] name = { "boxremove" };
        private String helptext = "Entfernt die exakt von dir genannte Box aus deiner Boxinfo, als Beispiel: \"!boxremove 7270v1\".";
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
            if (toolbox.getDatabaseByName("box.db").Remove(sender + ":" + message))
            {
                connection.sendmsg("Erledigt!", receiver);
            }
            else
            {
                connection.sendmsg("Der Suchstring wurde nicht gefunden und deshalb nicht gelöscht", receiver);
            }
        }
    }
}