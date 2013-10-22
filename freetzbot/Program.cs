using Db4objects.Db4o.Ext;
using FritzBot.Core;
using System;
using System.Collections.Generic;
using System.IO;
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
            #region Antiflooding checks
            if (!toolbox.IsOp(theMessage.TheUser))
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
                PluginInfo info = PluginManager.GetInstance().Get(theMessage.CommandName);

                if (info != null)
                {
                    if (!info.AuthenticationRequired || toolbox.IsOp(theMessage.TheUser))
                    {
                        if (!info.ParameterRequiredSpecified || (info.ParameterRequired && theMessage.HasArgs) || (!info.ParameterRequired && !theMessage.HasArgs))
                        {
                            try
                            {
                                ICommand command = info.GetScoped<ICommand>(theMessage.Data.Channel, theMessage.TheUser);
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
                    else if (theMessage.TheUser.Admin)
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
                        using (DBProvider db = new DBProvider())
                        {
                            User nutzer = db.GetUser(ConsoleSplitted[1]);
                            if (nutzer != null)
                            {
                                if (nutzer.Admin)
                                {
                                    Console.WriteLine(nutzer.LastUsedName + " ist bereits OP");
                                    break;
                                }
                                nutzer.Admin = true;
                                db.SaveOrUpdate(nutzer);
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
                        ServerManager.GetInstance().Remove(ServerManager.GetInstance()[ConsoleSplitted[1]]);
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

            try
            {
                DBProvider.Defragmentieren();
            }
            catch (Db4oIOException)
            {
                toolbox.Logging("Defragmentierung fehlgeschlagen, starte Workaround");
                DBProvider.Shutdown();
                File.Delete(DBProvider.DBPath);
                File.Move(DBProvider.DBPath + ".backup", DBProvider.DBPath);
                DBProvider.ReCreate();
                DBProvider.Defragmentieren();
            }

            PluginManager.GetInstance().BeginInit(true);

            int count = new DBProvider().Query<User>().Count();
            toolbox.Logging(count + " Benutzer geladen!");

            ServerManager Servers = ServerManager.GetInstance();
            if (Servers.ConnectionCount == 0)
            {
                Console.WriteLine("Keine Verbindungen bekannt, starte Verbindungsassistent");
                AskConnection();
            }
            Servers.ConnectAll();

            toolbox.SafeThreadStart("ConsolenThread", true, HandleConsoleInput);
        }

        private static void Deinit()
        {
            ServerManager.GetInstance().DisconnectAll();
            PluginManager.Shutdown();
            DBProvider.Shutdown();
        }

        private static void Main()
        {
            Init();
            ShutdownSignal.WaitOne();
            Deinit();
            Environment.Exit(RestartFlag ? 99 : 0);
        }
    }
}