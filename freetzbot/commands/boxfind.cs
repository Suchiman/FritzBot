using System;

namespace FritzBot.commands
{
    class boxfind : ICommand
    {
        public String[] Name { get { return new String[] { "boxfind" }; } }
        public String HelpText { get { return "Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\"."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            String besitzer = "";
            foreach (User oneuser in FritzBot.Program.TheUsers)
            {
                foreach (String box in oneuser.boxes)
                {
                    if (box.Contains(message))
                    {
                        besitzer += ", " + oneuser.names[0];
                        break;
                    }
                }
            }
            besitzer = besitzer.Remove(0, 2);
            if (!String.IsNullOrEmpty(besitzer))
            {
                connection.Sendmsg("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer, receiver);
            }
            else
            {
                connection.Sendmsg("Diese Box scheint niemand zu besitzen!", receiver);
            }
        }
    }
}