using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Name("taskcontrol")]
    [Help("Steuert meine Background Tasks: !taskcontrol <taskname> <start/stop>")]
    [ParameterRequired(true)]
    [Authorize]
    class taskcontrol : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.AnswerHelp(this);
                return;
            }
            PluginInfo bgtask = PluginManager.GetInstance().Get(theMessage.CommandArgs[0]);
            if (bgtask != null && bgtask.IsBackgroundTask)
            {
                if (theMessage.CommandArgs[1].ToLower() == "start")
                {
                    bgtask.Start();
                    theMessage.Answer("Task erfolgreich gestartet");
                }
                else if (theMessage.CommandArgs[1].ToLower() == "stop")
                {
                    bgtask.Stop();
                    theMessage.Answer("Task erfolgreich angehalten");
                }
                else
                {
                    theMessage.Answer("Ich habe die Aktion die auf den Task angewandt werden soll nicht verstanden");
                }
            }
            else
            {
                theMessage.Answer(String.Format("Ich habe keinen Task namens {0} finden können", theMessage.CommandArgs[0]));
            }
        }
    }
}