using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    class witz : ICommand
    {
        public String[] Name { get { return new String[] { "witz", "joke" }; } }
        public String HelpText { get { return "Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public witz()
        {
            WitzRandoms = new Queue<int>(10);
        }

        private Queue<int> WitzRandoms;

        public void Run(ircMessage theMessage)
        {
            String Joke = "";
            if (theMessage.hasArgs)
            {
                if (theMessage.CommandArgs[0].ToLower() == "add")
                {
                    theMessage.getUser.AddJoke(theMessage.CommandLine.Substring(theMessage.CommandLine.IndexOf(' ')));
                    theMessage.Answer("Ist notiert " + theMessage.Nick);
                }
                else
                {
                    Joke = GetRandom(GetSpecialJokes(theMessage.CommandArgs));
                    if (String.IsNullOrEmpty(Joke))
                    {
                        theMessage.Answer("Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet");
                        return;
                    }
                }
            }
            else
            {
                Joke = GetRandom(Program.TheUsers.AllJokes());
                if (String.IsNullOrEmpty(Joke))
                {
                    theMessage.Answer("Mir fällt gerade kein Fritz!Witz ein");
                    return;
                }
            }
            theMessage.Answer(Joke);
        }

        private List<String> GetSpecialJokes(List<String> Filter)
        {
            List<String> result = Program.TheUsers.AllJokes();
            for (int i = 0; i < result.Count; i++)
            {
                for (int x = 0; x < Filter.Count; x++)
                {
                    if (!result[i].ToLower().Contains(Filter[x].ToLower()))
                    {
                        result.RemoveAt(i);
                        i--;
                    }
                }
            }
            return result;
        }

        private String GetRandom(List<String> Jokes)
        {
            if (!(Jokes.Count > 0))
            {
                return "";
            }
            Random rand = new Random();
            if (WitzRandoms.Count >= 10)
            {
                WitzRandoms.Dequeue();
            }
            int random = rand.Next(Jokes.Count - 1);
            for (int i = 0; WitzRandoms.Contains(random) && i < 10; i++)
            {
                random = rand.Next(Jokes.Count - 1);
            }
            WitzRandoms.Enqueue(random);
            return Jokes[random];
        }
    }
}