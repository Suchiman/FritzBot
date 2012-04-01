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

        public void run(irc connection, String sender, String receiver, String message)
        {
            String[] Daten = toolbox.getDatabaseByName("box.db").GetContaining(message);
            if (Daten.Length > 0)
            {
                String besitzer = "";
                String[] temp;
                for (int i = 0; i < Daten.Length; i++)
                {
                    temp = Daten[i].Split(new String[] { ":" }, 2, StringSplitOptions.None);
                    if (besitzer == "")
                    {
                        besitzer = temp[0];
                    }
                    else
                    {
                        besitzer += ", " + temp[0];
                    }
                }
                connection.sendmsg("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer, receiver);
            }
            else
            {
                connection.sendmsg("Diese Box scheint niemand zu besitzen!", receiver);
            }
        }
    }
}