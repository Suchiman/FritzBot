using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using System;

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
            Program.UserNickChanged -= nick;
            Program.UserMessaged -= message;
        }

        public void Start()
        {
            Program.UserJoined += joined;
            Program.UserQuit += gone;
            Program.UserPart += gone;
            Program.UserNickChanged += nick;
            Program.UserMessaged += message;
        }

        private void joined(Join data)
        {
            UserManager.GetInstance()[data.Nickname].GetModulUserStorage(this).SetVariable("LastSeen", DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        }

        private void gone(IRCEvent data)
        {
            UserManager.GetInstance()[data.Nickname].GetModulUserStorage(this).SetVariable("LastSeen", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            UserManager.GetInstance()[data.Nickname].Authenticated = false;
        }

        private void nick(Nick data)
        {
            if (!UserManager.GetInstance().Exists(data.Nickname))
            {
                UserManager.GetInstance()[data.Nickname].AddName(data.NewNickname);
                UserManager.GetInstance()[data.Nickname].Authenticated = false;
            }
        }

        private void message(ircMessage theMessage)
        {
            if (!theMessage.Nickname.Contains(".") && theMessage.Nickname != theMessage.IRC.Nickname)
            {
                theMessage.TheUser.GetModulUserStorage(this).SetVariable("LastMessaged", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                theMessage.TheUser.GetModulUserStorage(this).SetVariable("LastMessage", theMessage.Message);
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandLine.ToLower() == theMessage.IRC.Nickname.ToLower())
            {
                theMessage.Answer("Ich bin gerade hier und laut meinem Logik System solltest du auch sehen können was ich schreibe");
                return;
            }
            if (UserManager.GetInstance().Exists(theMessage.CommandLine))
            {
                String output = "";
                DateTime LastSeen = DateTime.Parse(UserManager.GetInstance()[theMessage.CommandLine].GetModulUserStorage(this).GetVariable("LastSeen", DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:sszzz")));
                DateTime LastMessaged = DateTime.Parse(UserManager.GetInstance()[theMessage.CommandLine].GetModulUserStorage(this).GetVariable("LastMessaged", DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:sszzz")));
                if (LastSeen != DateTime.MinValue)
                {
                    output = "Den/Die habe ich hier zuletzt am " + LastSeen.ToString("dd.MM.yyyy ") + "um" + LastSeen.ToString(" HH:mm:ss ") + "Uhr gesehen.";
                }
                if (LastMessaged != DateTime.MinValue)
                {
                    if (!String.IsNullOrEmpty(output))
                    {
                        output += " ";
                    }
                    output += "Am " + LastMessaged.ToString("dd.MM.yyyy ") + "um" + LastMessaged.ToString(" HH:mm:ss ") + "Uhr sagte er/sie zuletzt: \"" + UserManager.GetInstance()[theMessage.CommandLine].GetModulUserStorage(this).GetVariable("LastMessage", "") + "\"";
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