using FritzBot.Core;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("frag")]
    [Module.Help("Dann werde ich den genannten Benutzer nach seiner Box fragen, z.B. !frag Anonymous")]
    [Module.ParameterRequired]
    class frag : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            Server.OnJoin += Server_OnJoin;
        }

        public void Stop()
        {
            Server.OnJoin -= Server_OnJoin;
        }

        void Server_OnJoin(object sender, JoinEventArgs e)
        {
            User u = new DBProvider().GetUser(e.Who);
            if (u == null || u.Ignored) return;
            boxfrage(e.Data.Irc, e.Who, true);
        }

        public void Run(ircMessage theMessage)
        {
            boxfrage(theMessage.Server.IrcClient, theMessage.CommandLine, false);
        }

        public void boxfrage(IrcClient connection, string nick, bool check_db = true)
        {
            try
            {
                using (DBProvider db = new DBProvider())
                {
                    User u = db.GetUser(nick);
                    SimpleStorage pluginStorage = GetPluginStorage(db);
                    SimpleStorage userStorage = db.GetSimpleStorage(u, PluginID);

                    if (check_db)
                    {
                        if (!pluginStorage.Get("BoxFrage", false) || userStorage.Get("asked", false)) return;
                        System.Threading.Thread.Sleep(10000);
                    }
                    connection.SendMessage(SendType.Message, nick, String.Format("Hallo {0}, ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !boxadd deine box, mitteilen, falls du dies nicht bereits getan hast :).", nick));
                    connection.SendMessage(SendType.Message, nick, "Pro !boxadd bitte nur eine Box nennen (nur die Boxversion) z.b. !boxadd 7270v1 oder !boxadd 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !boxadd 7270)");
                    userStorage.Store("asked", true);
                    db.SaveOrUpdate(userStorage);
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }
    }
}