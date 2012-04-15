using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    class witz : ICommand
    {
        public String[] Name { get { return new String[] { "witz" }; } }
        public String HelpText { get { return "Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        static private List<int> witz_randoms = new List<int>();

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                String[] witz = message.Split(new String[] { " " }, 2, StringSplitOptions.None);
                if (witz[0] == "add")
                {
                    FritzBot.Program.TheUsers[sender].AddJoke(witz[1]);
                    connection.Sendmsg("Ist notiert " + sender, receiver);
                }
                else
                {
                    String[] splitted = message.Split(' ');
                    List<String> alle_witze = FritzBot.Program.TheUsers.AllJokes();
                    List<String> such_witze = alle_witze;
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
                        connection.Sendmsg(such_witze[rand.Next(such_witze.Count)], receiver);
                    }
                    else
                    {
                        connection.Sendmsg("Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet", receiver);
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
                List<String> jokes = FritzBot.Program.TheUsers.AllJokes();
                int random = rand.Next(jokes.Count - 1);
                for (int i = 0; !(!witz_randoms.Contains(random) && i < 10); i++)
                {
                    random = rand.Next(jokes.Count - 1);
                }
                witz_randoms.Add(random);
                if (jokes.Count > 0)
                {
                    connection.Sendmsg(jokes[random], receiver);
                }
                else
                {
                    connection.Sendmsg("Mir fällt gerade kein Fritz!Witz ein", receiver);
                }
            }
        }
    }
}