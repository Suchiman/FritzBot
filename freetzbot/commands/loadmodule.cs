using System;
using System.IO;
using System.Net;
using System.Reflection;

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
                Type t = null;
                String name = theMessage.CommandLine;
                if (theMessage.CommandLine.Contains("http://"))
                {
                    name = theMessage.CommandLine.Substring(theMessage.CommandLine.LastIndexOf('/') + 1);
                    name = name.Substring(0, name.Length - 3);
                    try
                    {
                        WebClient Downloader = new WebClient();
                        Downloader.DownloadFile(theMessage.CommandLine, "plugins\\" + name + ".cs");
                    }
                    catch
                    {
                        theMessage.Answer("Beim Downloaden ist leider ein Fehler aufgetreten");
                        return;
                    }
                }
                if (File.Exists("plugins\\" + name + ".cs"))
                {
                    try
                    {
                        t = toolbox.LoadSource(new String[] { "plugins\\" + name + ".cs" }).GetType("FritzBot.commands." + name);
                    }
                    catch
                    {
                        theMessage.Answer("Das Kompilieren der Quelldatei ist leider fehlgeschlagen...");
                        return;
                    }
                }
                else
                {
                    t = Assembly.GetExecutingAssembly().GetType("FritzBot.commands." + name);
                }
                if (t == null)
                {
                    theMessage.Answer("Modul wurde nicht gefunden");
                    return;
                }
                try
                {
                    foreach (ICommand theCommand in Program.Commands)
                    {
                        if (theCommand.Name[0] == name)
                        {
                            theCommand.Destruct();
                            Program.Commands.Remove(theCommand);
                        }
                    }
                }
                catch { }
                Program.Commands.Add((ICommand)Activator.CreateInstance(t));
                Properties.Settings.Default.IgnoredModules.Remove(name);
                Properties.Settings.Default.Save();
                theMessage.Answer("Modul erfolgreich geladen");
            }
            catch
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
            }
        }
    }
}
