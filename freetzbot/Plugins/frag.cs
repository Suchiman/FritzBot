using FritzBot.Core;
using FritzBot.Database;
using FritzBot.DataModel;
using Meebey.SmartIrc4net;
using System;
using System.Threading;

namespace FritzBot.Plugins
{
    [Name("frag")]
    [Help("Dann werde ich den genannten Benutzer nach seiner Box fragen, z.B. !frag Anonymous")]
    [ParameterRequired]
    class frag : PluginBase, ICommand, IBackgroundTask
    {
        public void Start()
        {
            ServerConnection.OnJoin += Server_OnJoin;
        }

        public void Stop()
        {
            ServerConnection.OnJoin -= Server_OnJoin;
        }

        void Server_OnJoin(object sender, JoinEventArgs e)
        {
            using (var context = new BotContext())
            {
                User u = context.GetUser(e.Who);
                if (u == null || u.Ignored) return;
            }
            boxfrage(e.Data.Irc, e.Who, true);
        }

        public void Run(IrcMessage theMessage)
        {
            boxfrage(theMessage.ServerConnetion.IrcClient, theMessage.CommandLine, false);
        }

        public void boxfrage(IrcClient connection, string nick, bool check_db = true)
        {
            if (check_db && ConfigHelper.GetBoolean("BoxFrage", false))
            {
                return;
            }
            try
            {
                using (var context = new BotContext())
                {
                    User u = context.GetUser(nick);
                    UserKeyValueEntry entry = context.GetStorageOrCreate(u, "frag_asked");
                    if (check_db)
                    {
                        if (!ConfigHelper.GetBoolean("BoxFrage", false) || entry.Value == "true") return;
                        Thread.Sleep(10000);
                    }
                    connection.SendMessage(SendType.Message, nick, String.Format("Hallo {0}, ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !boxadd deine box, mitteilen, falls du dies nicht bereits getan hast :).", nick));
                    connection.SendMessage(SendType.Message, nick, "Pro !boxadd bitte nur eine Box nennen (nur die Boxversion) z.b. !boxadd 7270v1 oder !boxadd 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !boxadd 7270)");
                    entry.Value = "true";
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }
    }
}