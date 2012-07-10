using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections.Generic;

namespace FritzBot.commands
{
    class loadmodule : ICommand
    {
        public String[] Name { get { return new String[] { "modprobe", "insmod", "loadmodule" }; } }
        public String HelpText { get { return "Aktiviert einen meiner Befehle"; } }
        public Boolean OpNeeded { get { return true; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            try
            {
                String PluginDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
                String name = theMessage.CommandLine;
                Type[] AllTypes = null;
                List<Type> TypesToLoad = new List<Type>();

                if (name.Contains("http://"))
                {
                    name = theMessage.CommandLine.Substring(theMessage.CommandLine.LastIndexOf('/') + 1);
                    name = name.Substring(0, name.Length - 3);
                    try
                    {
                        WebClient Downloader = new WebClient();
                        Downloader.DownloadFile(theMessage.CommandLine, Path.Combine(PluginDirectory, name + ".cs"));
                    }
                    catch
                    {
                        theMessage.Answer("Beim Downloaden ist leider ein Fehler aufgetreten");
                        return;
                    }
                }
                if (File.Exists(Path.Combine(PluginDirectory, name + ".cs")))
                {
                    try
                    {
                        AllTypes = toolbox.LoadSource(new String[] { Path.Combine(PluginDirectory, name + ".cs") }).GetTypes();
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
                else
                {
                    AllTypes = Assembly.GetExecutingAssembly().GetTypes();
                    foreach (Type oneType in AllTypes)
                    {
                        if (oneType.Name != "ICommand" && (typeof(ICommand)).IsAssignableFrom(oneType) && (((ICommand)Activator.CreateInstance(oneType)).Name[0].ToLower() == name.ToLower()))
                        {
                            TypesToLoad.Add(oneType);
                        }
                    }
                }
                if (!(TypesToLoad.Count > 0))
                {
                    theMessage.Answer("Modul wurde nicht gefunden");
                    return;
                }
                TryAgain:
                try
                {
                    foreach (ICommand theCommand in Program.Commands)
                    {
                        foreach (Type oneType in TypesToLoad)
                        {
                            if (theCommand.GetType() == oneType)
                            {
                                theCommand.Destruct();
                                Program.Commands.Remove(theCommand);
                            }
                        }
                    }
                }
                catch { goto TryAgain; }
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
