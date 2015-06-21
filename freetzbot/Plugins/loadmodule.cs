using FritzBot.Core;
using FritzBot.DataModel;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace FritzBot.Plugins
{
    [Name("modprobe", "insmod", "loadmodule")]
    [Help("Lädt ein Plugin nach")]
    [ParameterRequired]
    [Authorize]
    class loadmodule : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            try
            {
                string PluginDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
                string name = theMessage.CommandLine;
                int loaded = 0;

                if (name.StartsWith("http://"))
                {
                    name = theMessage.CommandLine.Substring(theMessage.CommandLine.LastIndexOf('/') + 1);
                    try
                    {
                        WebClient Downloader = new WebClient();
                        Downloader.DownloadFile(theMessage.CommandLine, Path.Combine(PluginDirectory, name));
                    }
                    catch (Exception ex)
                    {
                        theMessage.Answer("Beim Downloaden ist leider ein Fehler aufgetreten");
                        Log.Error(ex, "Downloaden der Quelldatei fehlgeschlagen");
                        return;
                    }
                }

                if (name.Contains(".cs"))
                {
                    try
                    {
                        loaded = PluginManager.LoadPluginFromFile(Path.Combine(PluginDirectory, name));
                    }
                    catch (Exception ex)
                    {
                        theMessage.Answer("Das Kompilieren der Quelldatei ist leider fehlgeschlagen...");
                        Log.Error(ex, "Kompilieren der Quelldatei fehlgeschlagen");
                        return;
                    }
                }
                else
                {
                    loaded = PluginManager.LoadPluginByName(Assembly.GetExecutingAssembly(), name);
                }
                theMessage.Answer(String.Format("{0} Plugin{1} geladen", loaded, (loaded == 0 || loaded > 1) ? "s" : ""));
            }
            catch
            {
                theMessage.Answer("Das hat eine Exception ausgelöst");
            }
        }
    }
}