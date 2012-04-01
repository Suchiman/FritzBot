using System;

namespace freetzbot.commands
{
    class userlist : command
    {
        private String[] name = { "userlist" };
        private String helptext = "Das gibt eine Liste jener Benutzer aus, die mindestens eine Box bei mir registriert haben.";
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

        public void run(irc connection, String sender, String receiver, String message)
        {
            Boolean gefunden = false;
            String[] Daten = toolbox.getDatabaseByName("box.db").GetAll();
            String besitzer = "";
            String[] temp;
            foreach (String data in Daten)
            {
                temp = data.Split(new String[] { ":" }, 2, StringSplitOptions.None);
                if (!besitzer.Contains(temp[0]))
                {
                    if (besitzer == "")
                    {
                        besitzer = temp[0];
                        gefunden = true;
                    }
                    else
                    {
                        besitzer += ", " + temp[0];
                        gefunden = true;
                    }
                }
            }
            if (gefunden == true)
            {
                connection.sendmsg("Diese Benutzer haben bei mir mindestens eine Box registriert: " + besitzer, receiver);
            }
            else
            {
                connection.sendmsg("Ich fürchte, mir ist ein Fehler unterlaufen. Ich kann keine registrierten Benutzer feststellen.", receiver);
            }
        }
    }
}