﻿using System;

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
            String boxen = "";
            foreach (User oneuser in freetzbot.Program.TheUsers)
            {
                foreach (String box in oneuser.boxes)
                {
                    if (!boxen.Contains(box))
                    {
                        boxen += ", " + box;
                    }
                }
            }
            boxen = boxen.Remove(0, 2);
            connection.sendmsg("Folgende Boxen wurden bei mir registriert: " + boxen, receiver);
        }
    }
}