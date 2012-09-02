using System;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("rmmod", "unloadmodule")]
    [Module.Help("Deaktiviert einen meiner Befehle")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class unloadmodule : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                String UnloadModuleName = theMessage.CommandArgs[0];
                List<IBackgroundTask> tasksToUnload = new List<IBackgroundTask>();
                List<ICommand> commandsToUnload = new List<ICommand>();
                foreach (IBackgroundTask task in Program.BackgroundTasks)
                {
                    Module.NameAttribute name = toolbox.GetAttribute<Module.NameAttribute>(task);
                    if (name.IsNamed(UnloadModuleName))
                    {
                        tasksToUnload.Add(task);
                    }
                }
                foreach (ICommand command in Program.Commands)
                {
                    Module.NameAttribute name = toolbox.GetAttribute<Module.NameAttribute>(command);
                    if (name.IsNamed(UnloadModuleName))
                    {
                        commandsToUnload.Add(command);
                    }
                }
                foreach (IBackgroundTask task in tasksToUnload)
                {
                    task.Stop();
                    Program.BackgroundTasks.Remove(task);
                }
                foreach (ICommand command in commandsToUnload)
                {
                    Program.Commands.Remove(command);
                }
                if (commandsToUnload.Count > 0 || tasksToUnload.Count > 0)
                {
                    theMessage.Answer("Modul erfolgreich entladen");
                }
                else
                {
                    theMessage.Answer("Modul wurde nicht gefunden");
                }
            }
            catch (Exception ex)
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
                toolbox.Logging("Unloadmodule Exception " + ex.Message + ": " + ex.StackTrace);
            }
        }
    }
}
