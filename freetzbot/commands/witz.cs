using System;
using System.Collections.Generic;

namespace freetzbot.commands
{
    class witz : command
    {
        private String[] name = { "witz" };
        private String helptext = "Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen";
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

        static private List<int> witz_randoms = new List<int>();

        public void run(irc connection, String sender, String receiver, String message)
        {
            if (message != "")
            {
                String[] witz = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (witz[0] == "add")
                {
                    toolbox.getDatabaseByName("witze.db").Add(witz[1]);
                    connection.sendmsg("Ist notiert " + sender, receiver);
                }
                else
                {
                    String[] splitted = message.Split(' ');
                    List<String> alle_witze = new List<String>(toolbox.getDatabaseByName("witze.db").GetAll());
                    List<String> such_witze = new List<String>(alle_witze);
                    for (int i = 0; i < alle_witze.Count; i++)
                    {
                        foreach (String data in splitted)
                        {
                            if (!alle_witze[i].ToLower().Contains(data.ToLower()))
                            {
                                such_witze.Remove(alle_witze[i]);
                            }
                        }
                    }
                    if (such_witze.Count > 0)
                    {
                        Random rand = new Random();
                        connection.sendmsg(such_witze[rand.Next(such_witze.Count)], receiver);
                    }
                    else
                    {
                        connection.sendmsg("Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet", receiver);
                    }
                }
            }
            else
            {
                Random rand = new Random();
                if (witz_randoms.Count >= 10)
                {
                    witz_randoms.RemoveAt(0);
                }
                int random = rand.Next(toolbox.getDatabaseByName("witze.db").Size());
                for (int i = 0; !(!witz_randoms.Contains(random) && i < 10); i++)
                {
                    random = rand.Next(toolbox.getDatabaseByName("witze.db").Size());
                }
                witz_randoms.Add(random);
                if (toolbox.getDatabaseByName("witze.db").Size() > 0)
                {
                    connection.sendmsg(toolbox.getDatabaseByName("witze.db").GetAt(random), receiver);
                }
                else
                {
                    connection.sendmsg("Mir fällt gerade kein Fritz!Witz ein", receiver);
                }
            }
        }
    }
}