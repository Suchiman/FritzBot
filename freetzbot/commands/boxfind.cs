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

        public void Run(ircMessage theMessage)
        {
            String besitzer = "";
            foreach (User oneuser in Program.TheUsers)
            {
                foreach (String box in oneuser.boxes)
                {
                    if (box.Contains(theMessage.CommandLine))
                    {
                        besitzer += ", " + oneuser.names[0];
                        break;
                    }
                }
            }
            besitzer = besitzer.Remove(0, 2);
            if (!String.IsNullOrEmpty(besitzer))
            {
                theMessage.SendPrivateMessage("Folgende Benutzer scheinen diese Box zu besitzen: " + besitzer);
            }
            else
            {
                theMessage.SendPrivateMessage("Diese Box scheint niemand zu besitzen!");
            }
        }
    }
}