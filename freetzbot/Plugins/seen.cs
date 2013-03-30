using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("seen", "said")]
    [Module.Help("Gibt aus wann der Nutzer zuletzt gesehen wurde und wann er was zuletzt sagte.")]
    [Module.ParameterRequired]
    class seen : PluginBase, ICommand, IBackgroundTask
    {
        public void Stop()
        {
            Program.UserJoined -= joined;
            Program.UserQuit -= gone;
            Program.UserPart -= gone;
            Program.UserMessaged -= message;
        }

        public void Start()
        {
            Program.UserJoined += joined;
            Program.UserQuit += gone;
            Program.UserPart += gone;
            Program.UserMessaged += message;
        }

        private SeenEntry GetSeenEntry(DBProvider db, string nick)
        {
            User u = db.GetUser(nick);
            SeenEntry entry = db.QueryLinkedData<SeenEntry, User>(u).FirstOrDefault();
            if (entry == null)
            {
                entry = new SeenEntry();
                entry.Reference = u;
            }
            return entry;
        }

        private void joined(Join data)
        {
            using (DBProvider db = new DBProvider())
            {
                SeenEntry entry = GetSeenEntry(db, data.Nickname);
                entry.LastSeen = DateTime.MinValue;
                db.SaveOrUpdate(entry);
            }
        }

        private void gone(IRCEvent data)
        {
            using (DBProvider db = new DBProvider())
            {
                SeenEntry entry = GetSeenEntry(db, data.Nickname);
                entry.LastSeen = DateTime.Now;
                db.SaveOrUpdate(entry);
            }
        }

        private void message(ircMessage theMessage)
        {
            if (theMessage.IsIgnored)
            {
                return;
            }
            using (DBProvider db = new DBProvider())
            {
                SeenEntry entry = GetSeenEntry(db, theMessage.Nickname);
                entry.LastMessaged = DateTime.Now;
                entry.LastMessage = theMessage.Message;
                db.SaveOrUpdate(entry);
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandLine.ToLower() == theMessage.IRC.Nickname.ToLower())
            {
                theMessage.Answer("Ich bin gerade hier und laut meinem Logik System solltest du auch sehen können was ich schreibe");
                return;
            }
            using (DBProvider db = new DBProvider())
            {
                User u = db.GetUser(theMessage.CommandLine);
                if (u != null)
                {
                    SeenEntry entry = db.QueryLinkedData<SeenEntry, User>(u).FirstOrDefault();
                    string output = "";
                    if (entry != null)
                    {
                        if (entry.LastSeen != DateTime.MinValue)
                        {
                            output = "Den/Die habe ich hier zuletzt am " + entry.LastSeen.ToString("dd.MM.yyyy ") + "um" + entry.LastSeen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                        }
                        if (entry.LastMessaged != DateTime.MinValue)
                        {
                            if (!String.IsNullOrEmpty(output))
                            {
                                output += " ";
                            }
                            output += "Am " + entry.LastMessaged.ToString("dd.MM.yyyy ") + "um" + entry.LastMessaged.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + entry.LastMessage + "\"";
                        }
                    }
                    if (!String.IsNullOrEmpty(output))
                    {
                        theMessage.Answer(output);
                    }
                    else
                    {
                        theMessage.Answer("Scheinbar sind meine Datensätze unvollständig, tut mir leid");
                    }
                }
                else
                {
                    theMessage.Answer("Diesen Benutzer habe ich noch nie gesehen");
                }
            }
        }
    }
}