using System;

namespace freetzbot.commands
{
    class trunk : command
    {
        private String[] name = { "trunk" };
        private String helptext = "Dies zeigt den aktuellsten Changeset an.";
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
            String webseite = toolbox.get_web("http://freetz.org/changeset");
            if (webseite != "")
            {
                String changeset = "Der aktuellste Changeset ist " + webseite.Split(new String[] { "<h1>" }, 2, StringSplitOptions.None)[1].Split(new String[] { "</h1>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Changeset " }, 2, StringSplitOptions.None)[1];
                changeset += " und wurde am" + webseite.Split(new String[] { "<dd class=\"time\">" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "   " }, 5, StringSplitOptions.None)[4] + " in den Trunk eingecheckt. Siehe: http://freetz.org/changeset";
                connection.sendmsg(changeset, receiver);
            }
            else
            {
                connection.sendmsg("Leider war es mir nicht möglich auf die Freetz Webseite zuzugreifen", receiver);
            }
        }
    }
}