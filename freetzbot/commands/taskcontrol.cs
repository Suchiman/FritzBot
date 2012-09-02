using System;

namespace FritzBot.commands
{
    [Module.Name("taskcontrol")]
    [Module.Help("Steuert meine Background Tasks: !taskcontrol <taskname> <start/stop>")]
    [Module.ParameterRequired(true)]
    [Module.Authorize]
    class taskcontrol : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            foreach(IBackgroundTask task in Program.BackgroundTasks)
            {
                if (theMessage.CommandArgs.Count < 2)
                {
                    theMessage.AnswerHelp(this);
                    return;
                }
                Module.NameAttribute name = toolbox.GetAttribute<Module.NameAttribute>(task);
                if (name.IsNamed(theMessage.CommandArgs[0]))
                {
                    if (theMessage.CommandArgs[1] == "start")
                    {
                        task.Start();
                    }
                    else if (theMessage.CommandArgs[1] == "stop")
                    {
                        task.Stop();
                    }
                    else
                    {
                        theMessage.Answer("Ich habe die Aktion die auf den Task " + name.Names[0] + " angewandt werden soll nicht verstanden");
                    }
                }
            }
        }
    }
}