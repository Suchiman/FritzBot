using System;

namespace freetzbot.commands
{
    class frag : command
    {
        private String[] name = { "frag" };
        private String helptext = "Dann werde ich den genannten Benutzer nach seiner Box fragen, z.b. !frag Anonymous";
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
            boxfrage(connection, message, message, message, false);
        }

        public static void boxfrage(irc connection, String sender, String receiver, String message, Boolean check_db = true)
        {
            try
            {
                if (check_db)
                {
                    if (freetzbot.Program.configuration.get("boxfrage") == "false" || freetzbot.Program.TheUsers[sender].asked) return;
                    System.Threading.Thread.Sleep(10000);
                }
                connection.sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :).", receiver);
                connection.sendmsg("Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !box 7270) und achte darauf, dass du den Nicknamen trägst dem die Box zugeordnet werden soll", receiver);
            }
            catch (Exception ex)
            {
                toolbox.logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }
    }
}