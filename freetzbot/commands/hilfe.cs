using System;
using System.Collections.Generic;

namespace freetzbot.commands
{
    class hilfe : command
    {
        private String[] name = { "hilfe", "help", "faq", "info", "man" };
        private String helptext = "Die Hilfe!";
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
            if (message == "")
            {
                List<String> befehle = new List<String>();
                foreach (command thecommand in freetzbot.Program.commands)
                {
                    if (thecommand.get_op_needed() && toolbox.op_check(sender) || !thecommand.get_op_needed())
                    {
                        befehle.Add(thecommand.get_name()[0]);
                    }
                }
                befehle.Sort();
                String output = "";
                foreach (String data in befehle)
                {
                    output += ", " + data;
                }
                output = output.Remove(0, 2);
                connection.sendmsg("Derzeit verfügbare Befehle: " + output, receiver);
                connection.sendmsg("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen", receiver);
            }
            else
            {
                foreach (command thecommand in freetzbot.Program.commands)
                {
                    foreach (String name in thecommand.get_name())
                    {
                        if (message == name)
                        {
                            connection.sendmsg(thecommand.get_helptext(), receiver);
                            return;
                        }
                    }
                }
                connection.sendmsg("Ich konnte keinen Befehl finden der so heißt", receiver);
            }
        }
    }
}