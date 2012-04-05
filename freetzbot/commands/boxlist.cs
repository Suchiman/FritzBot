using System;

namespace freetzbot.commands
{
    class boxlist : command
    {
        private String[] name = { "boxlist" };
        private String helptext = "Dies listet alle registrierten Boxtypen auf.";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
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
            Boolean gefunden = false;
            String[] Daten = toolbox.getDatabaseByName("box.db").GetAll();
            String boxen = "";
            foreach (String data in Daten)
            {
                String[] temp = data.Split(new String[] { ":" }, 2, StringSplitOptions.None);
                if (!boxen.ToLower().Contains(temp[1].ToLower()))
                {
                    if (boxen == "")
                    {
                        boxen = temp[1];
                        gefunden = true;
                    }
                    else
                    {
                        boxen += ", " + temp[1];
                        gefunden = true;
                    }
                }
            }
            if (gefunden == true)
            {
                connection.sendmsg("Folgende Boxen wurden bei mir registriert: " + boxen, receiver);
            }
            else
            {
                connection.sendmsg("Da stimmt etwas nicht, es wurde bei mir keine Box registriert", receiver);
            }
        }
    }
}