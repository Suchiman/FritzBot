using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("remind")]
    [Module.Help("Hinterlasse einem Benutzer eine Nachricht. Sobald er wiederkommt oder etwas schreibt werde ich sie ihm Zustellen. !remind <Benutzer> <Nachricht>")]
    class remind : PluginBase, ICommand, IBackgroundTask
    {
        public void Stop()
        {
            Program.UserJoined -= SendJoin;
            Program.UserMessaged -= SendMessaged;
        }

        public void Start()
        {
            Program.UserJoined += SendJoin;
            Program.UserMessaged += SendMessaged;
        }

        private void SendJoin(Join data)
        {
            SendIt(UserManager.GetInstance()[data.Nickname], x => data.IRC.Sendmsg(x, data.Nickname));
        }

        public void SendMessaged(ircMessage theMessage)
        {
            SendIt(theMessage.TheUser, theMessage.SendPrivateMessage);
        }

        private void SendIt(User theUser, Action<String> SendAction)
        {
            List<XElement> AllUnread = theUser.GetModulUserStorage(this).Storage.Elements("reminder").ToList<XElement>();
            foreach (XElement OneUnread in AllUnread)
            {
                SendAction(OneUnread.Element("RememberNick").Value + " hat für dich am " + String.Format(DateTime.Parse(OneUnread.Element("RememberTime").Value).ToString("dd.MM.yyyy {0} HH:mm:ss"), "um") + " eine Nachricht hinterlassen: " + OneUnread.Element("RememberMessage").Value);
                OneUnread.Remove();
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count > 1)
            {
                if (UserManager.GetInstance().Exists(theMessage.CommandArgs[0]))
                {
                    UserManager.GetInstance()[theMessage.CommandArgs[0]].GetModulUserStorage(this).Storage.Add(new XElement("reminder",
                                new XElement("RememberNick", theMessage.Nickname),
                                new XElement("RememberMessage", theMessage.CommandLine.Substring(theMessage.CommandLine.IndexOf(' ') + 1)),
                                new XElement("RememberTime", DateTime.Now),
                                new XElement("Remembered", false)));
                    theMessage.Answer("Okay ich werde es sobald wie möglich zustellen");
                }
                else
                {
                    theMessage.Answer("Den Benutzer habe ich aber noch nie gesehen");
                }
            }
            else
            {
                theMessage.Answer("Die Eingabe war nicht korrekt: !remind <Benutzer> <Nachricht>");
            }
        }
    }
}