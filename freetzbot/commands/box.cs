using System;

namespace freetzbot.commands
{
    class box : command
    {
        private String[] name = { "box" };
        private String helptext = "Dies trägt deine Boxdaten ein, Beispiel: \"!box 7270\", bitte jede Box einzeln angeben.";
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
            if (freetzbot.Program.TheUsers[sender].AddBox(message))
            {
                connection.sendmsg("Okay danke, ich werde mir deine \"" + message + "\" notieren.", receiver);
                connection.sendmsg("Neue Box wurde registriert: User: " + sender + ", Box: " + message, "hippie2000");
            }
            else
            {
                connection.sendmsg("Wups, danke aber du hast mir deine \"" + message + "\" bereits mitgeteilt ;-).", receiver);
            }
        }
    }
}