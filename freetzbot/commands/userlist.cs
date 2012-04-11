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

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            String output = "";
            foreach (User oneuser in freetzbot.Program.TheUsers)
            {
                if (oneuser.boxes.Count > 0)
                {
                    output += ", " + oneuser.names[0];
                }
            }
            output = output.Remove(0, 2);
            if (output != "")
            {
                connection.sendmsg("Diese Benutzer haben bei mir mindestens eine Box registriert: " + output, receiver);
            }
            else
            {
                connection.sendmsg("Ich fürchte, mir ist ein Fehler unterlaufen. Ich kann keine registrierten Benutzer feststellen.", receiver);
            }
        }
    }
}