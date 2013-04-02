using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("frag")]
    [Module.Help("Dann werde ich den genannten Benutzer nach seiner Box fragen, z.B. !frag Anonymous")]
    [Module.ParameterRequired]
    class frag : PluginBase, ICommand, IBackgroundTask
    {
        public void Stop()
        {
            Program.UserJoined -= joined;
        }

        public void Start()
        {
            Program.UserJoined += joined;
        }

        private void joined(Join data)
        {
            User u = new DBProvider().GetUser(data.Nickname);
            if (u == null || u.Ignored) return;
            boxfrage(data.IRC, data.Nickname, data.Nickname, true);
        }

        public void Run(ircMessage theMessage)
        {
            boxfrage(theMessage.IRC, theMessage.CommandLine, theMessage.CommandLine, false);
        }

        public void boxfrage(Irc connection, string sender, string receiver, bool check_db = true)
        {
            try
            {
                using (DBProvider db = new DBProvider())
                {
                    User u = db.GetUser(sender);
                    SimpleStorage pluginStorage = GetPluginStorage(db);
                    SimpleStorage userStorage = db.GetSimpleStorage(u, PluginID);

                    if (check_db)
                    {
                        if (!pluginStorage.Get("BoxFrage", false) || userStorage.Get("asked", false)) return;
                        System.Threading.Thread.Sleep(10000);
                    }
                    connection.Sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !boxadd deine box, mitteilen, falls du dies nicht bereits getan hast :).", receiver);
                    connection.Sendmsg("Pro !boxadd bitte nur eine Box nennen (nur die Boxversion) z.b. !boxadd 7270v1 oder !boxadd 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !boxadd 7270)", receiver);
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