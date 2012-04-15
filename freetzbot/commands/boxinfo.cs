using System;

namespace FritzBot.commands
{
    class boxinfo : ICommand
    {
        public String[] Name { get { return new String[] { "boxinfo" }; } }
        public String HelpText { get { return "Zeigt die Box/en des angegebenen Benutzers an."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String output = "";
            if (String.IsNullOrEmpty(message))
            {
                message = sender;
            }
            if (FritzBot.Program.TheUsers.Exists(message))
            {
                foreach (String box in FritzBot.Program.TheUsers[message].boxes)
                {
                    output += ", " + box;
                }
            }
            else
            {
                connection.Sendmsg("Den habe ich hier noch nie gesehen, sry", receiver);
                return;
            }
            if (String.IsNullOrEmpty(output))
            {
                if (message == sender)
                {
                    connection.Sendmsg("Du hast bei mir noch keine Box registriert.", receiver);
                }
                else
                {
                    connection.Sendmsg("Über den habe ich keine Informationen.", receiver);
                }
                return;
            }
            output = output.Remove(0, 2);
            if (message == sender)
            {
                connection.Sendmsg("Du hast bei mir die Box/en " + output + " registriert.", receiver);
            }
            else
            {
                connection.Sendmsg(message + " sagte mir er/sie hätte die Box/en " + output, receiver);
            }
        }
    }
}