using FritzBot.Core;
using FritzBot.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
        public static void HandleCommand(ircMessage theMessage)
        {
            Contract.Requires(theMessage != null);
            bool isOp;
            bool isAdmin;
            using (var context = new BotContext())
            {
                User user = context.GetUser(theMessage.Nickname);
                isOp = toolbox.IsOp(user);
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

                if (info != null && info.IsCommand)
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
                                toolbox.Logging("Das Plugin " + info.Names.FirstOrDefault() + " hat eine nicht abgefangene Exception ausgelöst: " + ex.Message + "\r\n" + ex.StackTrace);
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
                toolbox.Logging("Eine Exception ist beim Ausführen eines Befehles abgefangen worden: " + ex.Message);
            }
        }

        private static void HandleConsoleInput()
        {
            while (true)
            {
                string ConsoleInput = Console.ReadLine();
                string[] ConsoleSplitted = ConsoleInput.Split(new[] { ' ' }, 2);
                switch (ConsoleSplitted[0])
                {
                    case "op":
                        if (ConsoleSplitted.Length < 2)
                        {
                            Console.WriteLine("Du musst den Benutzernamen angeben");
                            continue;
                        }
                        using (var context = new BotContext())
                        {
                            User nutzer = context.GetUser(ConsoleSplitted[1]);
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
                                Console.WriteLine("Benutzer " + ConsoleSplitted[1] + " nicht gefunden");
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
                        if (ConsoleSplitted.Length < 2)
                        {
                            Console.WriteLine("Du musst den Server angeben");
                            continue;
                        }
                        ServerManager.Remove(ServerManager.Servers.FirstOrDefault(x => x.Settings.Address == ConsoleSplitted[1]));
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
            Console.Write("Hostname: ");
            string Hostname = Console.ReadLine();
            int port = 0;
            do
            {
                Console.Write("Port: ");
            }
            while (!int.TryParse(Console.ReadLine(), out port));
            Console.Write("Nickname: ");
            string nickname = Console.ReadLine();
            Console.Write("QuitMessage: ");
            string QuitMessage = Console.ReadLine();
            Console.Write("InitialChannel: ");
            string channel = Console.ReadLine();
            try
            {
                toolbox.InstantiateConnection(Hostname, port, nickname, QuitMessage, channel);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Das war nicht erfolgreich: ");
                toolbox.Logging(ex);
            }
        }

        private static void Init()
        {
            RestartFlag = false;
            ShutdownSignal = new AutoResetEvent(false);

            PluginManager.BeginInit(true);

            //System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<BotContext, Configuration>());
            using (var context = new BotContext())
            {
                int count = context.Users.Count();
                toolbox.Logging(count + " Benutzer geladen!");
            }

            if (ServerManager.ConnectionCount == 0)
            {
                Console.WriteLine("Keine Verbindungen bekannt, starte Verbindungsassistent");
                AskConnection();
            }
            ServerManager.ConnectAll();

            toolbox.SafeThreadStart("ConsolenThread", true, HandleConsoleInput);
        }

        private static void Deinit()
        {
            ServerManager.DisconnectAll();
            PluginManager.Shutdown();
        }

        private static void Main()
        {
            new Plugins.fw().WorkerThread();
            Init();
            ShutdownSignal.WaitOne();
            Deinit();
            Environment.Exit(RestartFlag ? 99 : 0);
        }
    }
}