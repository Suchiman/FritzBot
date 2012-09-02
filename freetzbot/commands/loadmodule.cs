using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections.Generic;

namespace FritzBot.commands
{
    [Module.Name("modprobe", "insmod", "loadmodule")]
    [Module.Help("Aktiviert einen meiner Befehle")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class loadmodule : ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                String PluginDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
                String name = theMessage.CommandLine;
                Type[] AllTypes = null;
                List<Type> TypesToLoad = new List<Type>();

                #region HTTP Downloader
                if (name.Contains("http://"))
                {
                    name = theMessage.CommandLine.Substring(theMessage.CommandLine.LastIndexOf('/') + 1);
                    try
                    {
                        WebClient Downloader = new WebClient();
                        Downloader.DownloadFile(theMessage.CommandLine, Path.Combine(PluginDirectory, name));
                    }
                    catch
                    {
                        theMessage.Answer("Beim Downloaden ist leider ein Fehler aufgetreten");
                        return;
                    }
                }
                #endregion
                #region Quelldatei Laden
                if (name.Contains(".cs"))
                {
                    try
                    {
                        AllTypes = toolbox.LoadSource(Path.Combine(PluginDirectory, name)).GetTypes();
                    }
                    catch
                    {
                        theMessage.Answer("Das Kompilieren der Quelldatei ist leider fehlgeschlagen...");
                        return;
                    }
                    foreach (Type oneType in AllTypes)
                    {
                        if (oneType.Name != "ICommand" && (typeof(ICommand)).IsAssignableFrom(oneType))
                        {
                            TypesToLoad.Add(oneType);
                        }
                    }
                }
                #endregion
                else
                {
                    AllTypes = Assembly.GetExecutingAssembly().GetTypes();
                    foreach (Type oneType in AllTypes)
                    {
                        if (oneType.Name != "ICommand" && (typeof(ICommand)).IsAssignableFrom(oneType) && toolbox.GetAttribute<FritzBot.Module.NameAttribute>(oneType).IsNamed(name))
                        {
                            TypesToLoad.Add(oneType);
                        }
                    }
                }
                if (TypesToLoad.Count == 0)
                {
                    theMessage.Answer("Modul wurde nicht gefunden");
                    return;
                }
                try
                {
                    for (int i = 0; i < Program.Commands.Count; i++)
                    {
                        foreach (Type oneType in TypesToLoad)
                        {
                            if (Program.Commands[i].GetType() == oneType)
                            {
                                if (Program.Commands[i] is IBackgroundTask)
                                {
                                    (Program.Commands[i] as IBackgroundTask).Stop();
                                }
                                Program.Commands.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                catch { }
                foreach (Type oneType in TypesToLoad)
                {
                    ICommand theModule = (ICommand)Activator.CreateInstance(oneType);
                    Program.Commands.Add(theModule);
                    Properties.Settings.Default.IgnoredModules.Remove(oneType.Name);
                    Properties.Settings.Default.Save();
                }
                theMessage.Answer("Modul(e) erfolgreich geladen");
            }
            catch
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
            }
        }
    }
}