using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using FritzBot.Plugins;
using FritzBot.DataModel;
using System.Xml.Linq;
using System.Linq;
using System.ComponentModel;
using FritzBot.Core;

namespace FritzBot.Core
{
    public class UserManager : IEnumerable, IEnumerable<User>
    {
        private static UserManager instance;
        private List<User> TheUsers;
        private XElement UsersNode;

        public static UserManager GetInstance()
        {
            if (instance == null)
            {
                instance = new UserManager(XMLStorageEngine.GetManager().GetElement("Users"));
            }
            return instance;
        }

        private UserManager(XElement usersNode)
        {
            UsersNode = usersNode;
            TheUsers = new List<User>();
            foreach (XElement userNode in usersNode.Elements("User"))
            {
                TheUsers.Add(new User(userNode));
            }
        }

        public User this[string name]
        {
            get
            {
                if (name.Contains("#") || name.Contains(".") || String.IsNullOrEmpty(name))
                {
                    return new User(User.CreateUserNode());
                }
                User named = TheUsers.FirstOrDefault(x => x.names.Contains(name));
                if (named == null)
                {
                    named = Add(name);
                }
                return named;
            }
            set
            {
                for (int i = 0; i < TheUsers.Count; i++)
                {
                    if (TheUsers[i].names.Contains(name))
                    {
                        TheUsers[i] = value;
                    }
                }
            }
        }

        public User this[int index]
        {
            get
            {
                return TheUsers[index];
            }
            set
            {
                TheUsers[index] = value;
            }
        }

        public bool Exists(string name)
        {
            return TheUsers.SelectMany(x => x.names).Any(x => x.ToLower() == name.ToLower());
        }

        public User Add(string name)
        {
            if (TheUsers.FirstOrDefault(x => x.names.Contains(name)) == null)
            {
                XElement myNewUser = User.CreateUserNode();
                int id = 1;
                if (UsersNode.Elements("User").Count() > 0)
                {
                    id = TheUsers.Max(x => x.ID);
                }
                myNewUser.Add(new XAttribute("id", ++id));
                UsersNode.Add(myNewUser);
                User newUser = new User(myNewUser);
                newUser.AddName(name);
                TheUsers.Add(newUser);
                return newUser;
            }
            throw new Exception("Dieser User existiert bereits");
        }

        public User GetUserByID(int id)
        {
            return TheUsers.FirstOrDefault(x => x.ID == id);
        }

        public void Remove(string name)
        {
            User toRemvoe = TheUsers.FirstOrDefault(x => x.names.Contains(name));
            if (toRemvoe != null)
            {
                toRemvoe.RemoveUser();
                TheUsers.Remove(toRemvoe);
            }
        }

        public void Maintain()
        {
            //List<User> newUsers = new List<User>(TheUsers);
            //List<String> allNames = new List<String>();
            //List<String> doubleNames = new List<String>();
            //int UserCount = newUsers.Count;
            //for (int i = 0; i < UserCount; i++)
            //{
            //    int NamesCount = newUsers[i].names.Count;
            //    for (int x = 0; x < NamesCount; x++)
            //    {
            //        if (newUsers[i].names[x].Contains(".") || newUsers[i].names[x].Contains("#") || String.IsNullOrEmpty(newUsers[i].names[x]))
            //        {
            //            newUsers[i].names.RemoveAt(x);
            //            x--;
            //            NamesCount--;
            //        }
            //        else if (allNames.Contains(newUsers[i].names[x]))
            //        {
            //            doubleNames.Add(newUsers[i].names[x]);
            //        }
            //        else
            //        {
            //            allNames.Add(newUsers[i].names[x]);
            //        }
            //    }
            //    if (newUsers[i].names.Count == 0)
            //    {
            //        newUsers.RemoveAt(i);
            //        i--;
            //        UserCount--;
            //    }
            //}
            //newUsers.Sort();
            //TheUsers.Clear();
            //TheUsers = null;
            //TheUsers = newUsers;
        }

        public void GroupUser(string user1, string user2)
        {
            //User Fusioned = new User();
            //int u1 = -1, u2 = -1;
            //for (int i = 0; i < TheUsers.Count; i++)
            //{
            //    for (int x = 0; x < TheUsers[i].names.Count; x++)
            //    {
            //        if (TheUsers[i].names[x] == user1 && u2 == -1)
            //        {
            //            Fusioned = TheUsers[i];
            //            u2 = i;
            //            break;
            //        }
            //        if (TheUsers[i].names[x] == user2 && u1 == -1)
            //        {
            //            u1 = i;
            //            break;
            //        }
            //    }
            //}
            //if (u1 == -1 || u2 == -1)
            //{
            //    throw new ArgumentException("User not found");
            //}
            //foreach (string oldname in TheUsers[u1].names)
            //{
            //    Fusioned.AddName(oldname);
            //}
            //foreach (string oldbox in TheUsers[u1].boxes)
            //{
            //    Fusioned.AddBox(oldbox);
            //}
            //foreach (string oldjoke in TheUsers[u1].jokes)
            //{
            //    Fusioned.AddJoke(oldjoke);
            //}
            //if (TheUsers[u1].last_seen > Fusioned.last_seen)
            //{
            //    Fusioned.last_seen = TheUsers[u1].last_seen;
            //}
            //if (TheUsers[u1].last_messaged > Fusioned.last_messaged)
            //{
            //    Fusioned.last_messaged = TheUsers[u1].last_messaged;
            //    Fusioned.last_message = TheUsers[u1].last_message;
            //}
            //if (TheUsers[u1].ignored)
            //{
            //    Fusioned.ignored = true;
            //}
            //if (TheUsers[u1].asked)
            //{
            //    Fusioned.asked = true;
            //}
            //if (TheUsers[u1].IsOp)
            //{
            //    Fusioned.IsOp = true;
            //}
            //TheUsers.RemoveAt(u1);
            //TheUsers.RemoveAt(u2);
            //TheUsers.Add(Fusioned);
        }

        public int Count
        {
            get
            {
                return TheUsers.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return TheUsers.GetEnumerator();
        }

        IEnumerator<User> IEnumerable<User>.GetEnumerator()
        {
            return TheUsers.GetEnumerator();
        }
    }

    public class User : IComparable
    {
        private XElement UserNode;
        private XElement storage;
        public IEnumerable<string> names
        {
            get
            {
                return UserNode.Element("names").Elements("name").Select(x => x.Value);
            }
        }
        public string LastUsedNick
        {
            get
            {
                XElement element = UserNode.Element("names").Elements("name").FirstOrDefault(x => x.Attribute("LastUsed") != null);
                if (element != null)
                {
                    return element.Value;
                }
                else
                {
                    return names.FirstOrDefault();
                }
            }
            set
            {
                UserNode.Element("names").Elements("name").ForEach(x => x.Attributes("LastUsed").Remove());
                UserNode.Element("names").Elements("name").FirstOrDefault(x => x.Value == value).Add(new XAttribute("LastUsed", true));
            }
        }
        public int ID
        {
            get
            {
                return Convert.ToInt32(UserNode.Attribute("id").Value);
            }
        }
        public string password
        {
            get
            {
                return UserNode.Element("password") == null ? String.Empty : UserNode.Element("password").Value;
            }
            set
            {
                UserNode.Element("password").Value = value;
            }
        }
        public bool Authenticated;
        public bool ignored
        {
            get
            {
                return bool.Parse(UserNode.Element("ignored").Value);
            }
            set
            {
                UserNode.Element("ignored").Value = value.ToString();
            }
        }
        public bool IsOp
        {
            get
            {
                return bool.Parse(UserNode.Element("IsOp").Value);
            }
            set
            {
                UserNode.Element("IsOp").Value = value.ToString();
            }
        }
        public bool Online
        {
            get
            {
                return ServerManager.GetInstance().GetAllConnections().Any(x => x.Channels.Any(y => y.User.Contains(this)));
            }
        }

        public static XElement CreateUserNode()
        {
            XElement UserNode = new XElement("User",
                new XElement("names"),
                new XElement("password"),
                new XElement("ignored", "false"),
                new XElement("IsOp", "false"),
                new XElement("UserModulStorages"));
            return UserNode;
        }

        public User(XElement userNode)
        {
            UserNode = userNode;
            if (UserNode.Element("UserModulStorages") != null)
            {
                storage = UserNode.Element("UserModulStorages");
            }
            else
            {
                storage = new XElement("UserModulStorages");
                UserNode.Add(storage);
            }
        }

        void names_ListChanged(object sender, ListChangedEventArgs e)
        {
            UserNode.Element("names").RemoveNodes();
            foreach (string item in names.OrderBy(x => x))
            {
                UserNode.Element("names").Add(new XElement("name", item));
            }
        }

        public void SetPassword(string pw)
        {
            password = toolbox.Crypt(pw);
        }

        public bool CheckPassword(string pw)
        {
            if (password == toolbox.Crypt(pw))
            {
                return true;
            }
            return false;
        }

        public bool AddName(string name)
        {
            if (!names.Contains(name))
            {
                UserNode.Element("names").Add(new XElement("name", name));
                return true;
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            User CompareUser = obj as User;
            return names.ElementAt(0).CompareTo(CompareUser.names.ElementAt(0));
        }

        public void RemoveUser()
        {
            UserNode.Remove();
        }

        public ModulDataStorage GetModulUserStorage(object modul)
        {
            string id = "";
            if (modul is string)
            {
                id = (string)modul;
            }
            else
            {
                id = modul.GetType().Name;
            }
            XElement mstorage = storage.Elements("Storage").FirstOrDefault(x => x.Attribute("id") != null && x.Attribute("id").Value == id);
            if (mstorage == null)
            {
                storage.Add(new XElement("Storage", new XAttribute("id", id)));
                mstorage = storage.Elements("Storage").FirstOrDefault(x => x.Attribute("id") != null && x.Attribute("id").Value == id);
            }
            return new ModulDataStorage(mstorage);
        }

        public override string ToString()
        {
            return names.FirstOrDefault();
        }
    }
}