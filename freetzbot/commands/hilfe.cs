using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    class hilfe : ICommand
    {
        public String[] Name { get { return new String[] { "hilfe", "help", "faq", "info", "man", "lsmod" }; } }
        public String HelpText { get { return "Die Hilfe!"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return true; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            if (String.IsNullOrEmpty(message))
            {
                List<String> befehle = new List<String>();
                foreach (ICommand thecommand in FritzBot.Program.Commands)
                {
                    if (thecommand.OpNeeded && toolbox.OpCheck(sender) || !thecommand.OpNeeded)
                    {
                        befehle.Add(thecommand.Name[0]);
                    }
                }
                befehle.Sort();
                String output = "";
                foreach (String data in befehle)
                {
                    output += ", " + data;
                }
                output = output.Remove(0, 2);
                connection.Sendmsg("Derzeit verfügbare Befehle: " + output, receiver);
                connection.Sendmsg("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen", receiver);
            }
            else
            {
                foreach (ICommand thecommand in FritzBot.Program.Commands)
                {
                    foreach (String CommandName in thecommand.Name)
                    {
                        if (message == CommandName)
                        {
                            connection.Sendmsg(thecommand.HelpText, receiver);
                            return;
                        }
                    }
                }
                connection.Sendmsg("Ich konnte keinen Befehl finden der so heißt", receiver);
            }
        }
    }
}