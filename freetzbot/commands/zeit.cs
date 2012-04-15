using System;

namespace FritzBot.commands
{
    class zeit : ICommand
    {
        public String[] Name { get { return new String[] { "zeit" }; } }
        public String HelpText { get { return "Das gibt die aktuelle Uhrzeit aus."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            try
            {
                connection.Sendmsg("Laut meiner Uhr ist es gerade " + DateTime.Now.ToString("HH:mm:ss") + ".", receiver);
            }
            catch
            {
                connection.Sendmsg("Scheinbar ist meine Uhr kaputt, statt der Zeit habe ich nur eine Exception bekommen :(", receiver);
            }
        }
    }
}