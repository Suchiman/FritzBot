using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Xml.Linq;

namespace FritzBot.Core
{
    public class XMLStorageEngine
    {
        private const String databaseName = "database.xml";
        private static XMLStorageEngine xdm = null;
        private const int MostUpToDateVersion = 1;
        private bool databaseChanged = false;
        private XDocument database;
        private Timer saveTimer;

        private XMLStorageEngine()
        {
            if (File.Exists(databaseName + "_tmp"))
            {
                //Wenn die Datei hier existiert muss etwas beim Speichern schiefgegangen sein.
                //Die aktuelle Datenbank ist also wahrscheinlich korrumpiert, deshalb die aktuelle Datenbank als Fehlerhaft makieren und diese nehmen
                if (File.Exists(databaseName))
                {
                    File.Delete(databaseName + "_err");
                    File.Move(databaseName, databaseName + "_err");
                }
                File.Copy(databaseName + "_tmp", databaseName);
                File.Delete(databaseName + "_recovery");
                File.Move(databaseName + "_tmp", databaseName + "_recovery");
            }
            if (File.Exists(databaseName))
            {
                database = XDocument.Load(databaseName);
            }
            else if (File.Exists("users.db"))
            {
                database = XDocument.Parse(File.ReadAllText("users.db").Replace("&#x1;", ""));
                File.Move("users.db", "old_users.db");
            }
            database = GetMostUpToDateVersion(database);
            database.Changed += database_Changed;
            saveTimer = new Timer(60000);
            saveTimer.Elapsed += saveTimer_Elapsed;
            saveTimer.Start();
        }

        private void saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (databaseChanged)
            {
                Save();
                databaseChanged = false;
            }
        }

        /// <summary>
        /// Speichert die Datenbank auf die Festplatte
        /// </summary>
        public void Save()
        {
            //Zur Sicherheit Datenbank erst verschieben, dann speichern und die alte Datenbank dann löschen
            if (File.Exists(databaseName))
            {
                File.Delete(databaseName + "_tmp");
                File.Move(databaseName, databaseName + "_tmp");
            }
            database.Save(databaseName);
            File.Delete(databaseName + "_tmp");
        }

        public XDocument Database { get { return database; } }

        public static XMLStorageEngine GetManager()
        {
            if (xdm == null)
            {
                xdm = new XMLStorageEngine();
            }
            return xdm;
        }

        public static void Shutdown()
        {
            if (xdm != null)
            {
                xdm.saveTimer.Stop();
                xdm.Save();
            }
        }

        private void database_Changed(object sender, XObjectChangeEventArgs e)
        {
            databaseChanged = true;
        }

        public XElement GetElement(String name)
        {
            return database.Element("Fritzbot").Element(name);
        }

        public ModulDataStorage GetGlobalSettingsStorage(object plugin)
        {
            String id = "";
            if (plugin is String)
            {
                id = (String)plugin;
            }
            else
            {
                id = plugin.GetType().Name;
            }
            XElement StorageNode = database.Element("Fritzbot").Element("GlobalSettings");
            XElement PluginStorage = StorageNode.Elements("Storage").FirstOrDefault(x => x.Attribute("id") != null && x.Attribute("id").Value == id);
            if (PluginStorage == null)
            {
                StorageNode.Add(new XElement("Storage", new XAttribute("id", id)));
                PluginStorage = StorageNode.Elements("Storage").FirstOrDefault(x => x.Attribute("id") != null && x.Attribute("id").Value == id);
            }
            return new ModulDataStorage(PluginStorage);
        }

        private XDocument GetMostUpToDateVersion(XDocument xml)
        {
            XDocument xmldata = xml;
            int xmlversion = 0;
            if (xmldata == null)
            {
                xmldata = new XDocument(new XElement("Fritzbot", new XAttribute("version", "1"), new XElement("GlobalSettings"), new XElement("Servers"), new XElement("Users")));
                xmldata.Declaration = new XDeclaration("1.0", Encoding.UTF8.WebName, "yes");
            }
            xmlversion = SafeGetVersion(xmldata);
            while (MostUpToDateVersion > xmlversion)
            {
                switch (xmlversion)
                {
                    case 0:
                        xmldata = UpgradeToV1(xmldata);
                        toolbox.Logging("Datenbank Upgrade auf Version 1");
                        break;
                    default:
                        break;
                }
                xmlversion = SafeGetVersion(xmldata);
            }
            xmldata.Save(databaseName);
            return xmldata;
        }

        private int SafeGetVersion(XDocument doc)
        {
            int retvalue = 0;
            if (doc.Element("Fritzbot") != null && doc.Element("Fritzbot").Attribute("version") != null && int.TryParse(doc.Element("Fritzbot").Attribute("version").Value, out retvalue))
            {
                return retvalue;
            }
            return 0;
        }

        private XDocument UpgradeToV1(XDocument oldxml)
        {
            XDocument neu = new XDocument();
            neu.Declaration = new XDeclaration("1.0", Encoding.UTF8.WebName, "yes");
            neu.Add(new XElement("Fritzbot", new XAttribute("version", "1")));
            XElement root = neu.Element("Fritzbot");
            root.Add(new XElement("GlobalSettings"));
            root.Add(new XElement("Servers"));

            List<XElement> Users = new List<XElement>();
            int id = 1;
            foreach (XElement user in oldxml.Element("ArrayOfUser").Elements("User"))
            {
                XElement newUser = new XElement("User", new XAttribute("id", id++));

                XElement namen = new XElement("names");
                foreach (XElement name in user.Element("names").Elements("string").Where(x => !string.IsNullOrEmpty(x.Value)))
                {
                    namen.Add(new XElement("name", name.Value));
                }
                if (!namen.HasElements)
                {
                    id--;
                    continue; //Ein Nutzer ohne Namen kann nicht zugeordnet werden, verwerfen
                }
                newUser.Add(namen);

                if (user.Element("password") != null && !String.IsNullOrEmpty(user.Element("password").Value))
                {
                    newUser.Add(new XElement("password", user.Element("password").Value));
                }

                if (user.Element("ignored") != null && !String.IsNullOrEmpty(user.Element("ignored").Value))
                {
                    newUser.Add(new XElement("ignored", user.Element("ignored").Value));
                }

                if (user.Element("IsOp") != null && !String.IsNullOrEmpty(user.Element("IsOp").Value))
                {
                    newUser.Add(new XElement("IsOp", user.Element("IsOp").Value));
                }

                XElement UserModulStorages = new XElement("UserModulStorages");
                {
                    if (user.Element("boxes") != null && user.Element("boxes").Elements().Count() > 0)
                    {
                        XElement boxstorage = new XElement("Storage", new XAttribute("id", "box"));
                        foreach (XElement box in user.Element("boxes").Elements("string"))
                        {
                            boxstorage.Add(new XElement("box", box.Value));
                        }
                        UserModulStorages.AddIfHasElements(boxstorage);
                    }

                    if (user.Element("jokes") != null && user.Element("jokes").Elements("string").Count() > 0)
                    {
                        XElement witzstorage = new XElement("Storage", new XAttribute("id", "witz"));
                        foreach (XElement joke in user.Element("jokes").Elements("string"))
                        {
                            witzstorage.Add(new XElement("witz", joke.Value.Trim()));
                        }
                        UserModulStorages.AddIfHasElements(witzstorage);
                    }

                    if (user.Element("alias") != null && user.Element("alias").Element("alias") != null && user.Element("alias").Element("alias").Elements().Count() > 0)
                    {
                        XElement aliasstorage = new XElement("Storage", new XAttribute("id", "alias"));

                        List<String> aliasnamen = user.Element("alias").Element("alias").Elements().Select(x => x.Value).ToList<String>();
                        List<String> beschreibungen = user.Element("alias").Element("description").Elements().Select(x => x.Value).ToList<String>();
                        for (int i = 0; i < aliasnamen.Count; i++)
                        {
                            aliasstorage.Add(new XElement("alias", new XElement("name", aliasnamen[i]), new XElement("beschreibung", beschreibungen[i])));
                        }

                        UserModulStorages.AddIfHasElements(aliasstorage);
                    }

                    XElement seenstorage = new XElement("Storage", new XAttribute("id", "seen"));
                    {
                        seenstorage.Add(new XElement("LastSeen", user.Element("last_seen").Value));
                        seenstorage.Add(new XElement("LastMessaged", user.Element("last_messaged").Value));
                        seenstorage.Add(new XElement("LastMessage", user.Element("last_message").Value));
                    }
                    UserModulStorages.AddIfHasElements(seenstorage);

                    XElement remindstorage = new XElement("Storage", new XAttribute("id", "remind"));
                    {
                        List<String> RememberNicks = user.Element("RememberNick").Elements("string").Select(x => x.Value).ToList<String>();
                        List<String> RememberMessages = user.Element("RememberMessage").Elements("string").Select(x => x.Value).ToList<String>();
                        List<DateTime> RememberTimes = user.Element("RememberTime").Elements("dateTime").Select(x => DateTime.Parse(x.Value)).ToList<DateTime>();
                        List<bool> Remembereders = user.Element("Remembered").Elements("boolean").Select(x => Boolean.Parse(x.Value)).ToList<bool>();
                        for (int i = 0; i < RememberNicks.Count; i++)
                        {
                            XElement reminder = new XElement("reminder",
                                new XElement("RememberNick", RememberNicks[i]),
                                new XElement("RememberMessage", RememberMessages[i]),
                                new XElement("RememberTime", RememberTimes[i]),
                                new XElement("Remembered", Remembereders[i]));
                            remindstorage.Add(reminder);
                        }
                    }
                    UserModulStorages.AddIfHasElements(remindstorage);

                    XElement fragstorage = new XElement("Storage", new XAttribute("id", "frag"), new XElement("asked", user.Element("asked").Value));
                    UserModulStorages.Add(fragstorage);
                }
                newUser.Add(UserModulStorages);
                Users.Add(newUser);
            }

            if (File.Exists("norris.txt"))
            {
                string[] lines = File.ReadAllLines("norris.txt", Encoding.Default).Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToArray();
                XElement user = Users.FirstOrDefault(x => x.Element("names") != null && x.Element("names").Elements("name").Any(y => y.Value == "FritzBot"));
                if (user == null)
                {
                    user = User.CreateUserNode();
                    user.Element("names").Add(new XElement("name", "FritzBot"));
                    Users.Add(user);
                }
                XElement witz = user.Element("UserModulStorages").Elements("Storage").FirstOrDefault(x => x.AttributeValueOrEmpty("id") == "witz");
                if (witz == null)
                {
                    witz = new XElement("Storage", new XAttribute("id", "witz"));
                    user.Element("UserModulStorages").Add(witz);
                }
                foreach (string line in lines)
                {
                    witz.Add(new XElement("witz", line));
                }
            }

            root.Add(new XElement("Users", Users.ToArray()));

            return neu;
        }
    }
}