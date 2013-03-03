using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("witz", "joke")]
    [Module.Help("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen")]
    class witz : PluginBase, ICommand
    {
        public witz()
        {
            WitzRandoms = new Queue<int>(10);
        }

        private Queue<int> WitzRandoms;

        public void Run(ircMessage theMessage)
        {
            string Joke = "";
            if (theMessage.HasArgs)
            {
                if (theMessage.CommandArgs[0].ToLower() == "add")
                {
                    theMessage.TheUser.GetModulUserStorage(this).Storage.Add(new XElement("witz", String.Join(" ", theMessage.CommandArgs.Skip(1).ToArray())));
                    theMessage.Answer("Ist notiert " + theMessage.Nickname);
                }
                else
                {
                    Joke = GetRandom(GetSpecialJokes(theMessage.CommandArgs, UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage(this).Storage.Elements("witz")).Select(x => x.Value).ToList<string>()));
                    if (String.IsNullOrEmpty(Joke))
                    {
                        theMessage.Answer("Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet");
                        return;
                    }
                }
            }
            else
            {
                Joke = GetRandom(UserManager.GetInstance().SelectMany(x => x.GetModulUserStorage(this).Storage.Elements("witz")).Select(x => x.Value).ToList<string>());
                if (String.IsNullOrEmpty(Joke))
                {
                    theMessage.Answer("Mir fällt gerade kein Fritz!Witz ein");
                    return;
                }
            }
            theMessage.Answer(Joke);
        }

        private List<string> GetSpecialJokes(List<string> Filter, List<string> ToFilter)
        {
            for (int i = 0; i < ToFilter.Count; i++)
            {
                foreach (string filter in Filter)
                {
                    if (!ToFilter[i].ToLower().Contains(filter.ToLower()))
                    {
                        ToFilter.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            return ToFilter;
        }

        private string GetRandom(List<string> Jokes)
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