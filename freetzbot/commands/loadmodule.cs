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
                        if (oneType.Name != "ICommand" && (typeof(ICommand)).IsAssignableFrom(oneType) || oneType.Name != "IBackgroundTask" && (typeof(IBackgroundTask)).IsAssignableFrom(oneType))
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
                    foreach (Type oneType in TypesToLoad)
                    {
                        Module.NameAttribute nameAttr = toolbox.GetAttribute<Module.NameAttribute>(oneType);
                        for (int i = 0; i < Program.Commands.Count; i++)
                        {
                            Module.NameAttribute CName = toolbox.GetAttribute<Module.NameAttribute>(Program.Commands[i]);
                            if (CName != null && CName.Match(nameAttr))
                            {
                                Program.Commands.RemoveAt(i);
                                i--;
                            }
                        }
                        for (int i = 0; i < Program.BackgroundTasks.Count; i++)
                        {
                            Module.NameAttribute CName = toolbox.GetAttribute<Module.NameAttribute>(Program.BackgroundTasks[i]);
                            if (CName != null && CName.Match(nameAttr))
                            {
                                Program.BackgroundTasks.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                catch { }
                foreach (Type oneType in TypesToLoad)
                {
                    object instance = Activator.CreateInstance(oneType);
                    if (instance is ICommand)
                    {
                        Program.Commands.Add((ICommand)instance);
                    }
                    if (instance is IBackgroundTask)
                    {
                        IBackgroundTask task = (IBackgroundTask)instance;
                        task.Start();
                        Program.BackgroundTasks.Add(task);
                    }
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