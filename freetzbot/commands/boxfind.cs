using System;

namespace freetzbot.commands
{
    class boxfind : command
    {
        private String[] name = { "boxfind" };
        private String helptext = "Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".";
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
            String besitzer = "";
            foreach (User oneuser in freetzbot.Program.TheUsers)
            {
                foreach (String box in oneuser.boxes)
                {
                    if (box.Contains(message))
                    {
                        besitzer += ", " + oneuser.names[0];
                        break;
                    }
                }
            }
            besitzer = besitzer.Remove(0, 2);
            if (besitzer != "")
            {
                connection.sendmsg("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer, receiver);
            }
            else
            {
                connection.sendmsg("Diese Box scheint niemand zu besitzen!", receiver);
            }
        }
    }
}