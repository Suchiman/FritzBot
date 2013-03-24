using FritzBot.Core;
using FritzBot.DataModel;
using FritzBot.DataModel.IRC;
using FritzBot.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace FritzBot
{
    public class Program
    {
        #region Events
        public static event Action<Join> UserJoined;
        public static event Action<Part> UserPart;
        public static event Action<Quit> UserQuit;
        public static event Action<Nick> UserNickChanged;
        public static event Action<Kick> BotKicked;
        public static event Action<ircMessage> UserMessaged;
        #endregion

        public static bool restart;

        public static SimpleStorage BotSettings;
        private static int AntiFloodingCount;
        private static bool FloodingNotificated;

        private static void HandleCommand(ircMessage theMessage)
        {
            #region Antiflooding checks
            if (!toolbox.IsOp(theMessage.TheUser))
            {
                if (AntiFloodingCount >= BotSettings.Get("FloodingCount", 10))
                {
                    if (FloodingNotificated == false)
                    {
                        FloodingNotificated = true;
                        theMessage.Answer("Flooding Protection aktiviert");
                    }
                    return;
                }
                else
                {
                    AntiFloodingCount++;
                }
            }
            #endregion

            try
            {
                ICommand theCommand = PluginManager.GetInstance().Get<ICommand>(theMessage.CommandName);
                if (theCommand == null)
                {
                    string theAlias = FritzBot.Plugins.alias.GetAlias(theMessage);
                    if (!String.IsNullOrEmpty(theAlias))
                    {
                        theMessage.Answer(theAlias);
                    }
                }
                else
                {
                    bool OPNeeded = toolbox.GetAttribute<Module.AuthorizeAttribute>(theCommand) != null;
                    short ParameterNeeded = 0;
                    Module.ParameterRequiredAttribute ParameterAttri = toolbox.GetAttribute<Module.ParameterRequiredAttribute>(theCommand);
                    if (ParameterAttri != null)
                    {
                        if (ParameterAttri.Required)
                        {
                            ParameterNeeded = 1;
                        }
                        else
                        {
                            ParameterNeeded = 2;
                        }
                    }

                    if (!OPNeeded || toolbox.IsOp(theMessage.TheUser))
                    {
                        if (ParameterNeeded == 0 || (ParameterNeeded == 1 && theMessage.HasArgs) || (ParameterNeeded == 2 && !theMessage.HasArgs))
                        {
                            try
                            {
                                theCommand.Run(theMessage);
                            }
                            catch (Exception ex)
                            {
                                toolbox.Logging("Das Plugin " + toolbox.GetAttribute<Module.NameAttribute>(theCommand).Names[0] + " hat eine nicht abgefangene Exception ausgelöst: " + ex.Message + "\r\n" + ex.StackTrace);
                                theMessage.Answer("Oh... tut mir leid. Das Plugin hat einen internen Ausnahmefehler ausgelöst");
                            }
                        }
                        else
                        {
                            theMessage.Answer("Ungültiger Aufruf: " + toolbox.GetAttribute<Module.HelpAttribute>(theCommand).Help);
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

            if (!theMessage.Answered && theMessage.IsPrivate)
            {
                theMessage.Answer("Hallo, kann ich dir helfen ? Probiers doch mal mit !hilfe");
            }
        }

        public static void HandleIncomming(IRCEvent Daten)
        {
            if (Daten is ircMessage)
            {
                ircMessage message = Daten as ircMessage;
                if (!message.IsIgnored)
                {
                    RaiseUserMessaged(message);
                    if (message.IsCommand && !message.Handled)
                    {
                        try
                        {
                            HandleCommand(message);
                        }
                        catch (Exception ex)
                        {
                            toolbox.Logging(ex);
                        }
                    }
                    if (!message.Hidden)
                    {
                        if (message.IsPrivate)
                        {
                            toolbox.Logging("Von " + message.Source + ": " + message.Message);
                        }
                        else
                        {
                            toolbox.Logging(message.Source + " " + message.Nickname + ": " + message.Message);
                        }
                        foreach (string OneMessage in message.UnloggedMessages)
                        {
                            toolbox.Logging(OneMessage);
                        }
                    }
                    if (message.IsPrivate && !message.Answered)
                    {
                        if (message.IsCommand)
                        {
                            message.Answer("Den Befehl kenn ich nicht, schau dir doch mal die \"!help\" an");
                        }
                        else
                        {
                            message.Answer("Hallo du da, ich bin nicht so menschlich wie ich aussehe");
                        }
                    }
                }
            }
            else
            {
                toolbox.Logging(Daten.ToString());
                if (Daten is Join)
                {
                    RaiseUserJoined(Daten as Join);
                }
                if (Daten is Kick)
                {
                    RaiseBotKicked(Daten as Kick);
                }
                if (Daten is Nick)
                {
                    RaiseUserNickChanged(Daten as Nick);
                }
                if (Daten is Part)
                {
                    RaiseUserPart(Daten as Part);
                }
                if (Daten is Quit)
                {
                    RaiseUserQuit(Daten as Quit);
                }
            }
        }

        private static void RaiseUserMessaged(ircMessage theMessage)
        {
            Action<ircMessage> usermessaged = UserMessaged;
            if (usermessaged != null)
            {
                usermessaged(theMessage);
            }
        }

        private static void RaiseBotKicked(Kick data)
        {
            Action<Kick> kicked = BotKicked;
            if (kicked != null)
            {
                kicked(data);
            }
        }

        private static void RaiseUserNickChanged(Nick data)
        {
            Action<Nick> usernick = UserNickChanged;
            if (usernick != null)
            {
                usernick(data);
            }
        }

        private static void RaiseUserPart(Part data)
        {
            Action<Part> userpart = UserPart;
            if (userpart != null)
            {
                userpart(data);
            }
        }

        private static void RaiseUserQuit(Quit data)
        {
            Action<Quit> quitted = UserQuit;
            if (quitted != null)
            {
                quitted(data);
            }
        }

        private static void RaiseUserJoined(Join data)
        {
            Action<Join> userjoin = UserJoined;
            if (userjoin != null)
            {
                userjoin(data);
            }
        }

        private static void AntiFlooding()
        {
            while (true)
            {
                Thread.Sleep(BotSettings.Get("FloodingCountReduction", 1000));
                if (AntiFloodingCount > 0)
                {
                    AntiFloodingCount--;
                }
                if (AntiFloodingCount == 0)
                {
                    FloodingNotificated = false;
                }
            }
        }

        private static void HandleConsoleInput()
        {
            while (true)
            {
                string ConsoleInput = Console.ReadLine();
                string[] ConsoleSplitted = ConsoleInput.Split(new string[] { " " }, 2, StringSplitOptions.None);
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
                                    Console.WriteLine(nutzer.Names.ElementAt(0) + " ist bereits OP");
                                    break;
                                }
                                nutzer.Admin = true;
                                db.SaveOrUpdate(nutzer);
                                Console.WriteLine(nutzer.Names.ElementAt(0) + " zum OP befördert");
                            }
                            else
                            {
                                Console.WriteLine("Benutzer " + ConsoleSplitted[1] + " nicht gefunden");
                            }
                        }
                        break;
                    case "exit":
                        ServerManager.GetInstance().DisconnectAll();
                        Environment.Exit(0);
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
            toolbox.InstantiateConnection(Hostname, port, nickname, QuitMessage, channel);
        }

        private static void ConvertXMLToDB4o()
        {
            XDocument doc = XDocument.Load("database.xml");
            using (DBProvider db = new DBProvider())
            {
                XElement globalSettings = doc.Element("Fritzbot").Element("GlobalSettings");
                XElement boxdb = globalSettings.Elements("Storage").FirstOrDefault(x => x.Attribute("id").Value == "BoxDatabase");
                foreach (XElement xmlbox in boxdb.Elements("Box"))
                {
                    Box b = new Box();
                    b.ShortName = xmlbox.Element("ShortName").Value;
                    b.FullName = xmlbox.Element("FullName").Value;
                    foreach (XElement regexp in xmlbox.Element("Regex").Elements("Pattern"))
                    {
                        b.AddRegex(regexp.Value);
                    }
                    db.SaveOrUpdate(b);
                }

                XElement users = doc.Element("Fritzbot").Element("Users");
                foreach (XElement xmluser in users.Elements("User"))
                {
                    User u = new User();
                    foreach (XElement xmlname in xmluser.Element("names").Elements("name"))
                    {
                        if (xmlname.Attribute("LastUsed") != null)
                        {
                            u.LastUsedName = xmlname.Value;
                        }
                        u.AddName(xmlname.Value);
                    }
                    if (String.IsNullOrEmpty(u.LastUsedName))
                    {
                        u.LastUsedName = u.Names.FirstOrDefault();
                    }
                    u.Ignored = xmluser.Element("ignored").Value == "true";
                    u.Admin = xmluser.Element("IsOp").Value == "true";
                    u.Password = xmluser.Element("password") != null ? xmluser.Element("password").Value : String.Empty;
                    db.SaveOrUpdate(u);

                    foreach (XElement xmlstorage in xmluser.Element("UserModulStorages").Elements("Storage").Where(x => x.HasElements && x.Attribute("id") != null))
                    {
                        switch (xmlstorage.Attribute("id").Value)
                        {
                            case "seen":
                                {
                                    SeenEntry entry = new SeenEntry();
                                    entry.Reference = u;
                                    XElement LastMessaged = xmlstorage.Element("LastMessaged");
                                    if (LastMessaged != null)
                                    {
                                        entry.LastMessaged = DateTime.Parse(LastMessaged.Value);
                                    }
                                    XElement LastMessage = xmlstorage.Element("LastMessage");
                                    if (LastMessage != null)
                                    {
                                        entry.LastMessage = LastMessage.Value;
                                    }
                                    XElement LastSeen = xmlstorage.Element("LastSeen");
                                    if (LastSeen != null)
                                    {
                                        entry.LastSeen = DateTime.Parse(LastSeen.Value);
                                    }
                                    db.SaveOrUpdate(entry);
                                }
                                break;
                            case "frag":
                                {
                                    SimpleStorage fragstorage = db.GetSimpleStorage(u, "frag");
                                    fragstorage.Store("asked", xmlstorage.Element("asked").Value == "true");
                                    db.SaveOrUpdate(fragstorage);
                                }
                                break;
                            case "alias":
                                {
                                    foreach (XElement xmlalias in xmlstorage.Elements("alias"))
                                    {
                                        AliasEntry entry = new AliasEntry();
                                        entry.Creator = u;
                                        entry.Key = xmlalias.Element("name").Value;
                                        entry.Text = xmlalias.Element("beschreibung").Value;
                                        db.SaveOrUpdate(entry);
                                    }
                                }
                                break;
                            case "box":
                                {
                                    BoxEntry entry = new BoxEntry();
                                    entry.Reference = u;
                                    foreach (string xmlbox in xmlstorage.Elements("box").Where(x => !String.IsNullOrEmpty(x.Value)).Select(x => x.Value))
                                    {
                                        entry.AddBox(xmlbox);
                                    }
                                    db.SaveOrUpdate(entry);
                                }
                                break;
                            case "subscribe":
                                {
                                    if (xmlstorage.Element("Subscriptions") != null && xmlstorage.Element("Subscriptions").HasElements)
                                    {
                                        foreach (XElement xmlsub in xmlstorage.Element("Subscriptions").Elements("Plugin"))
                                        {
                                            Subscription entry = new Subscription();
                                            entry.Reference = u;
                                            entry.Plugin = xmlsub.Value;
                                            entry.Provider = xmlsub.Attribute("Provider").Value;
                                            if (xmlsub.Attribute("Bedingung") != null)
                                            {
                                                string bedingungen = xmlsub.Attribute("Bedingung").Value;
                                                if (bedingungen.Contains(";"))
                                                {
                                                    entry.Bedingungen.AddRange(bedingungen.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                                                }
                                                else
                                                {
                                                    entry.Bedingungen.Add(bedingungen);
                                                }
                                            }
                                            db.SaveOrUpdate(entry);
                                        }
                                    }
                                    if (xmlstorage.Element("Settings") != null && xmlstorage.Element("Settings").HasElements)
                                    {
                                        foreach (XElement xmlset in xmlstorage.Element("Settings").Elements("Provider"))
                                        {
                                            SimpleStorage settings = db.GetSimpleStorage(u, "SubscriptionSettings");
                                            settings.Store(xmlset.Attribute("Name").Value, xmlset.Value);
                                            db.SaveOrUpdate(settings);
                                        }
                                    }
                                }
                                break;
                            case "witz":
                                {
                                    foreach (string xmlwitz in xmlstorage.Elements("witz").Where(x => !String.IsNullOrEmpty(x.Value)).Select(x => x.Value))
                                    {
                                        WitzEntry entry = new WitzEntry();
                                        entry.Reference = u;
                                        entry.Witz = xmlwitz;
                                        db.SaveOrUpdate(entry);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static void Init()
        {
            restart = false;

            if (File.Exists("database.xml"))
            {
                Stopwatch sw = Stopwatch.StartNew();
                ConvertXMLToDB4o();
                sw.Stop();
                toolbox.Logging(String.Format("Datenbank konvertiert in {0} Millisekunden", sw.ElapsedMilliseconds));
                File.Move("database.xml", "backup_database.xml");
            }

            if (File.Exists("datenbank.db.backup"))
            {
                File.Delete("datenbank.db.backup");
            }

            DBProvider.Defragmentieren();

            BotSettings = new DBProvider().GetSimpleStorage("Bot");

            PluginManager.GetInstance().BeginInit(true);

            int count = new DBProvider().Query<User>().Count();
            toolbox.Logging(count + " Benutzer geladen!");

            ServerManager Servers = ServerManager.GetInstance();
            Servers.MessageReceivedEvent += HandleIncomming;
            if (Servers.ConnectionCount == 0)
            {
                Console.WriteLine("Keine Verbindungen bekannt, starte Verbindungsassistent");
                AskConnection();
            }
            Servers.ConnectAll();

            toolbox.SafeThreadStart("ConsolenThread", true, HandleConsoleInput);
            AntiFloodingCount = 0;
            toolbox.SafeThreadStart("AntifloodingThread", true, AntiFlooding);
        }

        private static void Deinit()
        {
            PluginManager.Shutdown();
            DBProvider.Shutdown();
        }

        private static void Main()
        {
            Init();
            while (ServerManager.GetInstance().Connected)
            {
                Thread.Sleep(2000);
            }
            Deinit();
            if (restart == true)
            {
                Environment.Exit(99);
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}