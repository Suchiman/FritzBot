using System;

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
                for (int i = 0; i < Program.Commands.Count; i++)
                {
                    if (toolbox.GetAttribute<Module.NameAttribute>(Program.Commands[i]).IsNamed(UnloadModuleName))
                    {
                        if (Program.Commands[i] is IBackgroundTask)
                        {
                            (Program.Commands[i] as IBackgroundTask).Stop();
                        }
                        Program.Commands[i] = null;
                        Program.Commands.RemoveAt(i);
                        Properties.Settings.Default.IgnoredModules.Add(UnloadModuleName);
                        Properties.Settings.Default.Save();
                        theMessage.Answer("Modul erfolgreich entladen");
                    }
                }
                if (!theMessage.Answered)
                {
                    theMessage.Answer("Modul wurde nicht gefunden");
                }
            }
            catch (Exception ex)
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
                toolbox.Logging("Unloadmodule Exception " + ex.Message);
            }
        }
        /*
        public void Run(ircMessage theMessage)
        {
            try
            {
                for (int i = 0; i < Program.Commands.Count; i++)
                {
                    if (toolbox.GetModuleIdentification(Program.Commands[i]).IsNamed(theMessage.CommandArgs[0]))
                    {
                        Program.Commands[i].Destruct();
                        Program.Commands[i] = null;
                        Program.Commands.RemoveAt(i);
                        Properties.Settings.Default.IgnoredModules.Add(theMessage.CommandArgs[0]);
                        Properties.Settings.Default.Save();
                        theMessage.Answer("Modul erfolgreich entladen");
                        return;
                    }
                }
                theMessage.Answer("Modul wurde nicht gefunden");
            }
            catch (Exception ex)
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
                toolbox.Logging("Unloadmodule Exception " + ex.Message);
            }
        }*/
    }
}
