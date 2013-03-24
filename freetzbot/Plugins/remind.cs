using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using System;
using System.Linq;

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
            SendIt(new DBProvider().GetUser(data.Nickname), x => data.IRC.Sendmsg(x, data.Nickname));
        }

        public void SendMessaged(ircMessage theMessage)
        {
            SendIt(theMessage.TheUser, theMessage.SendPrivateMessage);
        }

        private void SendIt(User theUser, Action<string> SendAction)
        {
            using (DBProvider db = new DBProvider())
            {
                foreach (ReminderEntry item in db.QueryLinkedData<ReminderEntry, User>(theUser).ToList())
                {
                    SendAction(item.Creator.LastUsedName + " hat für dich am " + item.Created.ToString("dd.MM.yyyy 'um' HH:mm:ss") + " eine Nachricht hinterlassen: " + item.Message);
                    db.Remove(item);
                }
            }
        }

        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count > 1)
            {
                using (DBProvider db = new DBProvider())
                {
                    User u = db.GetUser(theMessage.CommandArgs[0]);
                    if (u != null)
                    {
                        ReminderEntry r = new ReminderEntry();
                        r.Created = DateTime.Now;
                        r.Creator = theMessage.TheUser;
                        r.Message = String.Join(" ", theMessage.CommandArgs.Skip(1));
                        r.Reference = u;
                        db.SaveOrUpdate(r);
                        theMessage.Answer("Okay ich werde es sobald wie möglich zustellen");
                    }
                    else
                    {
                        theMessage.Answer("Den Benutzer habe ich aber noch nie gesehen");
                    }
                }
            }
            else
            {
                theMessage.Answer("Die Eingabe war nicht korrekt: !remind <Benutzer> <Nachricht>");
            }
        }
    }

    public class ReminderEntry : LinkedData<User>
    {
        public User Creator { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
    }
}