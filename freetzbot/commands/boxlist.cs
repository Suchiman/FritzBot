using System;

namespace FritzBot.commands
{
    [Module.Name("boxlist")]
    [Module.Help("Dies listet alle registrierten Boxtypen auf.")]
    [Module.ParameterRequired(false)]
    class boxlist : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            String boxen = "";
            foreach (User oneuser in Program.TheUsers)
            {
                foreach (String box in oneuser.boxes)
                {
                    if (!boxen.Contains(box))
                    {
                        boxen += ", " + box;
                    }
                }
            }
            boxen = boxen.Remove(0, 2);
            theMessage.Answer("Folgende Boxen wurden bei mir registriert: " + boxen);
        }
    }
}