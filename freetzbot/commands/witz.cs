using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FritzBot.commands
{
    [Module.Name("witz", "joke")]
    [Module.Help("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen")]
    class witz : ICommand
    {
        public witz()
        {
            WitzRandoms = new Queue<int>(10);
        }

        private Queue<int> WitzRandoms;

        public void Run(ircMessage theMessage)
        {
            String Joke = "";
            if (theMessage.HasArgs)
            {
                if (theMessage.CommandArgs[0].ToLower() == "norris" || theMessage.CommandArgs[0].ToLower() == "chuck")
                {
                    ChuckJokes(theMessage);
                    return;
                }
                if (theMessage.CommandArgs[0].ToLower() == "add")
                {
                    theMessage.TheUser.AddJoke(theMessage.CommandLine.Substring(theMessage.CommandLine.IndexOf(' ')));
                    theMessage.Answer("Ist notiert " + theMessage.Nick);
                }
                else
                {
                    Joke = GetRandom(GetSpecialJokes(theMessage.CommandArgs, theMessage.TheUsers.AllJokes()));
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

        private void ChuckJokes(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count > 1)
            {
                if (theMessage.CommandArgs[1] == "add")
                {
                    try
                    {
                        File.AppendAllText("norris.txt", "\r\n", Encoding.GetEncoding("iso-8859-1"));
                        File.AppendAllText("norris.txt", String.Join(" ", theMessage.CommandArgs.ToArray(), 2, theMessage.CommandArgs.Count - 2), Encoding.GetEncoding("iso-8859-1"));
                        theMessage.Answer("Niemand verscherzt es sich mit Chuck Norris! Witz hinzugefügt ;-)");
                    }
                    catch
                    {
                        theMessage.Answer("Das hat nicht funktioniert :(");
                    }
                    return;
                }
            }
            List<String> allJokes = null;
            try
            {
                allJokes = new List<String>(File.ReadAllLines("norris.txt", Encoding.GetEncoding("iso-8859-1")));
            }
            catch
            {
                theMessage.Answer("Scheint so als kenne ich gar keine Chuck Norris witze :-O");
                return;
            }
            theMessage.CommandArgs.RemoveAt(0);
            String Joke = GetRandom(GetSpecialJokes(theMessage.CommandArgs, allJokes));
            if (String.IsNullOrEmpty(Joke))
            {
                theMessage.Answer("Tut mir leid, ich kenne keinen Zutreffenden Chuck Norris Witz");
                return;
            }
            theMessage.Answer(Joke);
        }

        private List<String> GetSpecialJokes(List<String> Filter, List<String> ToFilter)
        {
            for (int i = 0; i < ToFilter.Count; i++)
            {
                for (int x = 0; x < Filter.Count; x++)
                {
                    if (!ToFilter[i].ToLower().Contains(Filter[x].ToLower()))
                    {
                        ToFilter.RemoveAt(i);
                        i--;
                    }
                }
            }
            return ToFilter;
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