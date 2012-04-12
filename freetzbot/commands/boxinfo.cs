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

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            String output = "";
            if (message == "")
            {
                message = sender;
            }
            if (freetzbot.Program.TheUsers.Exists(message))
            {
                foreach (String box in freetzbot.Program.TheUsers[message].boxes)
                {
                    output += ", " + box;
                }
            }
            else
            {
                connection.sendmsg("Den habe ich hier noch nie gesehen, sry", receiver);
                return;
            }
            if (output == "")
            {
                if (message == sender)
                {
                    connection.sendmsg("Du hast bei mir noch keine Box registriert.", receiver);
                }
                else
                {
                    connection.sendmsg("Über den habe ich keine Informationen.", receiver);
                }
                return;
            }
            output = output.Remove(0, 2);
            if (message == sender)
            {
                connection.sendmsg("Du hast bei mir die Box/en " + output + " registriert.", receiver);
            }
            else
            {
                connection.sendmsg(message + " sagte mir er/sie hätte die Box/en " + output, receiver);
            }
        }
    }
}