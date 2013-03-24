using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("witz", "joke")]
    [Module.Help("Ich werde dann einen Witz erzählen, mit \"!witz add witztext\" kannst du einen neuen Witz hinzufügen. Mit !witz stichwort kannst du einen speziellen Witz suchen")]
    class witz : PluginBase, ICommand
    {
        public witz()
        {
            WitzRandoms = new Queue<WitzEntry>(10);
        }

        private Queue<WitzEntry> WitzRandoms;

        public void Run(ircMessage theMessage)
        {
            string Joke = "";
            if (theMessage.HasArgs)
            {
                if (theMessage.CommandArgs[0].ToLower() == "add")
                {
                    WitzEntry w = new WitzEntry() { Witz = String.Join(" ", theMessage.CommandArgs.Skip(1)), Reference = theMessage.TheUser };
                    using (DBProvider db = new DBProvider())
                    {
                        db.SaveOrUpdate(w);
                    }
                    theMessage.Answer("Ist notiert " + theMessage.Nickname);
                }
                else
                {
                    using (DBProvider db = new DBProvider())
                    {
                        Joke = GetRandom(GetSpecialJokes(theMessage.CommandArgs, db.Query<WitzEntry>()));
                        if (String.IsNullOrEmpty(Joke))
                        {
                            theMessage.Answer("Tut mir leid ich kenne leider keinen Witz der alle deine Stichwörter beinhaltet");
                            return;
                        }
                    }

                }
            }
            else
            {
                using (DBProvider db = new DBProvider())
                {
                    Joke = GetRandom(db.Query<WitzEntry>());
                    if (String.IsNullOrEmpty(Joke))
                    {
                        theMessage.Answer("Mir fällt gerade kein Fritz!Witz ein");
                        return;
                    }
                }
            }
            theMessage.Answer(Joke);
        }

        private IQueryable<WitzEntry> GetSpecialJokes(List<string> Filter, IQueryable<WitzEntry> ToFilter)
        {
            return ToFilter.Where(x => Filter.All(f => x.Witz.Contains(f)));
        }

        private string GetRandom(IQueryable<WitzEntry> Jokes)
        {
            int count = Jokes.Count();
            if (!(count > 0))
            {
                return "";
            }
            Random rand = new Random();
            if (WitzRandoms.Count >= 10)
            {
                WitzRandoms.Dequeue();
            }
            WitzEntry witz = null;
            int zähler = 0;
            do
            {
                witz = Jokes.ElementAtOrDefault(rand.Next(count));
                zähler++;
            }
            while (WitzRandoms.Contains(witz) && zähler < 11);
            WitzRandoms.Enqueue(witz);
            return witz.Witz;
        }
    }

    public class WitzEntry : LinkedData<User>
    {
        public string Witz { get; set; }
    }
}