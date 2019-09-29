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
        public void Run(IrcMessage theMessage)
        {
            if (theMessage.CommandArgs.Count < 2)
            {
                theMessage.AnswerHelp(this);
                return;
            }
            if (PluginManager.Get(theMessage.CommandArgs[0]) is { IsBackgroundTask: true } bgtask)
            {
                if (theMessage.CommandArgs[1].Equals("start", StringComparison.OrdinalIgnoreCase))
                {
                    bgtask.Start();
                    theMessage.Answer("Task erfolgreich gestartet");
                }
                else if (theMessage.CommandArgs[1].Equals("stop", StringComparison.OrdinalIgnoreCase))
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
                theMessage.Answer($"Ich habe keinen Task namens {theMessage.CommandArgs[0]} finden können");
            }
        }
    }
}