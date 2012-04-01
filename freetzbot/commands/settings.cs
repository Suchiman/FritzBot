using System;

namespace freetzbot.commands
{
    class settings : command
    {
        private String[] name = { "settings" };
        private String helptext = "Ändert meine Einstellungen, Operator Befehl: !settings get name, !settings set name wert";
        private Boolean op_needed = true;
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
            String[] split = message.Split(' ');
            switch (split[0])
            {
                case "set":
                    try
                    {
                        freetzbot.Program.configuration.set(split[1], split[2]);
                    }
                    catch
                    {
                        connection.sendmsg("Wups, das hat eine Exception ausgelöst", receiver);
                        return;
                    }
                    connection.sendmsg("Okay", receiver);
                    break;
                case "get":
                    try
                    {
                        connection.sendmsg(freetzbot.Program.configuration.get(split[1]), receiver);
                    }
                    catch
                    {
                        connection.sendmsg("Wups, das hat eine Exception ausgelöst", receiver);
                        return;
                    }
                    break;
                default:
                    connection.sendmsg("Wups, da stimmt wohl etwas mit der Syntax nicht", receiver);
                    break;
            }
        }
    }
}