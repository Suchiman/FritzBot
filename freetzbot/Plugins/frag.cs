using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using System;
using System.Linq;

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
            if (toolbox.IsIgnored(data.Nickname)) return;
            boxfrage(data.IRC, data.Nickname, data.Nickname);
        }

        public void Run(ircMessage theMessage)
        {
            boxfrage(theMessage.IRC, theMessage.CommandLine, theMessage.CommandLine, false);
        }

        public void boxfrage(Irc connection, String sender, String receiver, bool check_db = true)
        {
            try
            {
                if (check_db)
                {
                    if ((PluginStorage.GetVariable("BoxFrage", "false") == "false") || UserManager.GetInstance()[sender].GetModulUserStorage("frag").GetVariable("asked", "false") == "true" || UserManager.GetInstance()[sender].GetModulUserStorage("box").Storage.Elements("box").Count() > 0) return;
                    System.Threading.Thread.Sleep(10000);
                }
                connection.Sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !boxadd deine box, mitteilen, falls du dies nicht bereits getan hast :).", receiver);
                connection.Sendmsg("Pro !boxadd bitte nur eine Box nennen (nur die Boxversion) z.b. !boxadd 7270v1 oder !boxadd 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !boxadd 7270)", receiver);
                UserManager.GetInstance()[sender].GetModulUserStorage("frag").SetVariable("asked", "true");
            }
            catch (Exception ex)
            {
                toolbox.Logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }
    }
}