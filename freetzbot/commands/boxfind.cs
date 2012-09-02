using System;

namespace FritzBot.commands
{
    [Module.Name("boxfind")]
    [Module.Help("Findet die Nutzer der angegebenen Box: Beispiel: \"!boxfind 7270\".")]
    [Module.ParameterRequired]
    class boxfind : ICommand
    {
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