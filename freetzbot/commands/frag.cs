using System;

namespace FritzBot.commands
{
    class frag : ICommand
    {
        public String[] Name { get { return new String[] { "frag" }; } }
        public String HelpText { get { return "Dann werde ich den genannten Benutzer nach seiner Box fragen, z.b. !frag Anonymous"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {
            Program.UserJoined -= new Program.JoinEventHandler(joined);
        }

        public frag()
        {
            Program.UserJoined += new Program.JoinEventHandler(joined);
        }

        private void joined(Irc connection, String nick, String Room)
        {
            if (toolbox.IsIgnored(nick)) return;
            boxfrage(connection, nick, nick);
        }

        public void Run(ircMessage theMessage)
        {
            boxfrage(theMessage.Connection, theMessage.CommandLine, theMessage.CommandLine, false);
        }

        public void boxfrage(Irc connection, String sender, String receiver, Boolean check_db = true)
        {
            try
            {
                if (check_db)
                {
                    if (!Properties.Settings.Default.BoxFrage || Program.TheUsers[sender].asked || Program.TheUsers[sender].boxes.Count > 0) return;
                    System.Threading.Thread.Sleep(10000);
                }
                connection.Sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !boxadd deine box, mitteilen, falls du dies nicht bereits getan hast :).", receiver);
                connection.Sendmsg("Pro !boxadd bitte nur eine Box nennen (nur die Boxversion) z.b. !boxadd 7270v1 oder !box 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !boxadd 7270)", receiver);
                Program.TheUsers[sender].asked = true;
            }
            catch (Exception ex)
            {
                toolbox.Logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }
    }
}