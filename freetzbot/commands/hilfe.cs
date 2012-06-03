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

        public void Run(ircMessage theMessage)
        {
            if (!theMessage.hasArgs)
            {
                List<String> befehle = new List<String>();
                foreach (ICommand thecommand in Program.Commands)
                {
                    if (thecommand.OpNeeded && toolbox.IsOp(theMessage.Nick) || !thecommand.OpNeeded)
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
                theMessage.Answer("Derzeit verfügbare Befehle: " + output);
                theMessage.Answer("Hilfe zu jedem Befehl mit \"!help befehl\". Um die anderen nicht zu belästigen kannst du mich auch per PM (query) anfragen");
            }
            else
            {
                foreach (ICommand thecommand in Program.Commands)
                {
                    foreach (String CommandName in thecommand.Name)
                    {
                        if (theMessage.CommandLine.ToLower() == CommandName.ToLower())
                        {
                            theMessage.Answer(thecommand.HelpText);
                            return;
                        }
                    }
                }
                theMessage.Answer("Ich konnte keinen Befehl finden der so heißt");
            }
        }
    }
}