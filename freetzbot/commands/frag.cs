using System;
using FritzBot;

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

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            boxfrage(connection, message, message, false);
        }

        public void boxfrage(Irc connection, String sender, String receiver, Boolean check_db = true)
        {
            try
            {
                if (check_db)
                {
                    if (Program.configuration["boxfrage"] == "false" || Program.TheUsers[sender].asked) return;
                    System.Threading.Thread.Sleep(10000);
                }
                connection.Sendmsg("Hallo " + sender + " , ich interessiere mich sehr für Fritz!Boxen, wenn du eine oder mehrere hast kannst du sie mir mit !box deine box, mitteilen, falls du dies nicht bereits getan hast :).", receiver);
                connection.Sendmsg("Pro !box bitte nur eine Box nennen (nur die Boxversion) z.b. !box 7270v1 oder !box 7170. Um die anderen im Channel nicht zu stören, sende es mir doch bitte per query/private Nachricht (z.b. /msg FritzBot !box 7270) und achte darauf, dass du den Nicknamen trägst dem die Box zugeordnet werden soll", receiver);
                Program.TheUsers[sender].asked = true;
            }
            catch (Exception ex)
            {
                toolbox.Logging("Da ist etwas beim erfragen der Box schiefgelaufen:" + ex.Message);
            }
        }
    }
}