using FritzBot.Core;
using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("taskcontrol")]
    [Module.Help("Steuert meine Background Tasks: !taskcontrol <taskname> <start/stop>")]
    [Module.ParameterRequired(true)]
    [Module.Authorize]
    class taskcontrol : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.AnswerHelp(this);
                return;
            }
            IBackgroundTask bgtask = PluginManager.GetInstance().Get<IBackgroundTask>(theMessage.CommandArgs[0]);
            if (bgtask != null)
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