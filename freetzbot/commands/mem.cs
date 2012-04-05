using System;

namespace freetzbot.commands
{
    class mem : command
    {
        private String[] name = { "mem" };
        private String helptext = "Meine aktuelle Speicherlast berechnet vom GC (Gargabe Collector) und die insgesamt Last";
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
            connection.sendmsg("GC Totalmem: " + GC.GetTotalMemory(true).ToString() + "Byte, WorkingSet: " + (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024).ToString() + "kB", receiver);
        }
    }
}