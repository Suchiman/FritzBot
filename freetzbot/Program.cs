using FritzBot.Core;
using FritzBot.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;

namespace FritzBot
{
    public static class Program
    {
        private static AutoResetEvent ShutdownSignal;
        private static bool RestartFlag;

        private static bool FloodingNotificated;
        private static List<DateTime> Floodings = new List<DateTime>();

        /// <summary>
        /// Verarbeitet eine ircMessage mit einem Command und stellt sicher, dass die dem Command zugeordneten Attribute eingehalten werden
        /// </summary>
        /// <param name="theMessage">Die zu verarbeitende ircMessage</param>
        public static void HandleCommand(IrcMessage theMessage)
        {
            Contract.Requires(theMessage != null);
            bool isOp;
            bool isAdmin;
            using (var context = new BotContext())
            {
                User user = context.GetUser(theMessage.Nickname);
                isOp = Toolbox.IsOp(user);
                isAdmin = user.Admin;
            }

            #region Antiflooding checks
            if (!isOp)
            {
                Floodings.RemoveAll(x => x < DateTime.Now.AddSeconds(-30));
                if (Floodings.Count >= ConfigHelper.GetInt("FloodingCount", 10))
                {
                    if (!FloodingNotificated)
                    {
                        FloodingNotificated = true;
                        theMessage.Answer("Flooding Protection aktiviert");
                    }
                    return;
                }
                if (Floodings.Count == 0)
                {
                    FloodingNotificated = false;
                }
                Floodings.Add(DateTime.Now);
            }
            #endregion

            try
            {
                PluginInfo info = PluginManager.Get(theMessage.CommandName);

                if (info?.IsCommand ?? false)
                {
                    if (!info.AuthenticationRequired || isOp)
                    {
                        if (!info.ParameterRequiredSpecified || (info.ParameterRequired && theMessage.HasArgs) || (!info.ParameterRequired && !theMessage.HasArgs))
                        {
                            try
                            {
                                ICommand command = info.GetScoped<ICommand>(theMessage.Data.Channel, theMessage.Nickname);
                                command.Run(theMessage);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Das Plugin {PluginName} hat eine nicht abgefangene Exception ausgelöst", info.Names.FirstOrDefault());
                                theMessage.Answer("Oh... tut mir leid. Das Plugin hat einen internen Ausnahmefehler ausgelöst");
                            }
                            theMessage.ProcessedByCommand = true;
                        }
                        else
                        {
                            theMessage.Answer("Ungültiger Aufruf: " + info.HelpText);
                        }
                    }
                    else if (isAdmin)
                    {
                        theMessage.Answer("Du musst dich erst authentifizieren, " + theMessage.Nickname);
                    }
                    else
                    {
                        theMessage.Answer("Du bist nicht dazu berechtigt, diesen Befehl auszuführen, " + theMessage.Nickname);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Eine Exception ist beim Ausführen eines Befehles abgefangen worden");
            }
        }

        private static void HandleConsoleInput()
        {
            while (true)
            {
                string consoleInput = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(consoleInput))
                {
                    continue;
                }

                string[] consoleSplitted = consoleInput.Split(new[] { ' ' }, 2);
                switch (consoleSplitted[0])
                {
                    case "op":
                        if (consoleSplitted.Length < 2)
                        {
                            Console.WriteLine("Du musst den Benutzernamen angeben");
                            continue;
                        }
                        using (var context = new BotContext())
                        {
                            User nutzer = context.GetUser(consoleSplitted[1]);
                            if (nutzer != null)
                            {
                                if (nutzer.Admin)
                                {
                                    Console.WriteLine(nutzer.LastUsedName + " ist bereits OP");
                                    break;
                                }
                                nutzer.Admin = true;
                                context.SaveChanges();
                                Console.WriteLine(nutzer.LastUsedName + " zum OP befördert");
                            }
                            else
                            {
                                Console.WriteLine("Benutzer " + consoleSplitted[1] + " nicht gefunden");
                            }
                        }
                        break;
                    case "exit":
                        Shutdown();
                        break;
                    case "connect":
                        AskConnection();
                        break;
                    case "leave":
                        if (consoleSplitted.Length < 2)
                        {
                            Console.WriteLine("Du musst den Server angeben");
                            continue;
                        }
                        ServerManager.Remove(ServerManager.Servers.FirstOrDefault(x => x.Settings.Address == consoleSplitted[1]));
                        break;
                    case "list":
                        Console.WriteLine("Verbunden mit den Servern: {0}", ServerManager.Servers.Select(x => x.Settings.Address).Join(", "));
                        break;
                    case "reconnect":
                        foreach (ServerConnection srv in ServerManager.Servers)
                        {
                            Console.WriteLine("Reconnecte {0}", srv.Settings.Address);
                            srv.Disconnect();
                            srv.Connect();
                        }
                        break;
                }
            }
        }

        public static void Shutdown(bool restart = false)
        {
            RestartFlag = restart;
            ShutdownSignal.Set();
        }

        private static void AskConnection()
        {
            string Question(string question)
            {
                Console.Write(question);
                return Console.ReadLine();
            }
            string Hostname = Question("Hostname: ");
            int port = 0;
            do
            {
                Console.Write("Port: ");
            }
            while (!int.TryParse(Console.ReadLine(), out port));
            string nickname = Question("Nickname: ");
            string QuitMessage = Question("QuitMessage: ");
            string channel = Question("InitialChannel: ");
            try
            {
                Toolbox.InstantiateConnection(Hostname, port, nickname, QuitMessage, channel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Das war nicht erfolgreich: ");
                Log.Error(ex, "Fehler beim Herstellen einer Verbindung zum IRC Server");
            }
        }

        private static void Init()
        {
            RestartFlag = false;
            ShutdownSignal = new AutoResetEvent(false);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Timestamp:dd.MM HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.File(path: Path.Combine("Logs", "log.txt"), outputTemplate: "{Timestamp:dd.MM.yyyy HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}", fileSizeLimitBytes: 20 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: null)
                .CreateLogger();

            PluginManager.BeginInit(true);

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
                SQLitePCL.raw.FreezeProvider();
            }

            using (var context = new BotContext())
            {
                context.Database.Migrate();
                int count = context.Users.Count();
                Log.Information("{UserCount} Benutzer geladen!", count);
            }

            if (ServerManager.ConnectionCount == 0)
            {
                Console.WriteLine("Keine Verbindungen bekannt, starte Verbindungsassistent");
                AskConnection();
            }
            ServerManager.ConnectAll();

            Toolbox.SafeThreadStart("ConsolenThread", true, HandleConsoleInput);
        }

        private static void Deinit()
        {
            ServerManager.DisconnectAll();
            PluginManager.Shutdown();
        }

        private static void Main()
        {
            var profiles = new DirectoryInfo("Profiles");
            if (!profiles.Exists)
            {
                profiles.Create();
            }
            AssemblyLoadContext.Default.SetProfileOptimizationRoot(profiles.FullName);
            AssemblyLoadContext.Default.StartProfileOptimization("Default");

            Init();
            ShutdownSignal.WaitOne();
            Deinit();
            Environment.Exit(RestartFlag ? 99 : 0);
        }
    }
}