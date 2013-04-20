﻿using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace FritzBot.Plugins
{
    [Module.Name("modprobe", "insmod", "loadmodule")]
    [Module.Help("Lädt ein Plugin nach")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class loadmodule : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
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
                    catch
                    {
                        theMessage.Answer("Beim Downloaden ist leider ein Fehler aufgetreten");
                        return;
                    }
                }

                if (name.Contains(".cs"))
                {
                    try
                    {
                        loaded = PluginManager.GetInstance().LoadPluginFromFile(Path.Combine(PluginDirectory, name));
                    }
                    catch
                    {
                        theMessage.Answer("Das Kompilieren der Quelldatei ist leider fehlgeschlagen...");
                        return;
                    }
                }
                else
                {
                    loaded = PluginManager.GetInstance().LoadPluginByName(Assembly.GetExecutingAssembly(), name);
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