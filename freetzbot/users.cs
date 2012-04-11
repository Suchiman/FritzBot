using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Xml;
using System.Text;

namespace freetzbot
{
    public class UserCollection : IEnumerable
    {
        private List<User> TheUsers;
        private Thread AutoFlushThread;

        public User this[String name]
        {
            get
            {
                foreach (User theuser in TheUsers)
                {
                    foreach (String onename in theuser.names)
                    {
                        if (onename == name)
                        {
                            return theuser;
                        }
                    }
                }
                Add(name);
                return TheUsers[TheUsers.Count - 1];
            }
            set
            {
                for (int i = 0; i < TheUsers.Count; i++)
                {
                    for (int x = 0; x < TheUsers[i].names.Count; x++)
                    {
                        if (TheUsers[i].names[x] == name)
                        {
                            TheUsers[i] = value;
                        }
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

        public UserCollection()
        {
            TheUsers = new List<User>();
            if (File.Exists("user.db") && !File.Exists("users.db"))
            {
                ConvertOld.StartConvert(this);
            }
            else
            {
                Reload();
            }
            AutoFlushThread = new Thread(new ThreadStart(this.AutoFlush));
            AutoFlushThread.IsBackground = true;
            AutoFlushThread.Start();
        }

        ~UserCollection()
        {
            Flush();
        }

        private void AutoFlush()
        {
            int sleeptime = 180000;
            while (true)
            {
                int converttime;
                if (int.TryParse(freetzbot.Program.configuration.get("UserAutoFlushIntervall"), out converttime))
                {
                    if (converttime != 0)
                    {
                        sleeptime = converttime;
                    }
                }
                Thread.Sleep(sleeptime);
                Flush();
            }
        }

        public Boolean Exists(String name)
        {
            foreach (User theuser in TheUsers)
            {
                foreach (String onename in theuser.names)
                {
                    if (onename == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Flush()
        {
            XmlTextWriter UDB = new XmlTextWriter("users.db", Encoding.GetEncoding("iso-8859-1"));
            XmlSerializer serializer = new XmlSerializer(TheUsers.GetType());
            serializer.Serialize(UDB, TheUsers);
            UDB.Close();
        }

        public void Reload()
        {
            if (File.Exists("users.db"))
            {
                FileInfo FI = new FileInfo("users.db");
                if (FI.Length > 0)
                {
                    StreamReader UDB = new StreamReader("users.db", Encoding.GetEncoding("iso-8859-1"));
                    XmlSerializer serializer = new XmlSerializer(TheUsers.GetType());
                    TheUsers = (List<User>)serializer.Deserialize(UDB);
                    UDB.Close();
                    foreach (User oneuser in TheUsers)
                    {
                        oneuser.authenticated = false;
                        oneuser.authcookiedate = DateTime.MinValue;
                    }
                }
            }
        }

        public void Add(String name)
        {
            List<User> CheckUser = new List<User>(TheUsers);
            foreach (User theuser in CheckUser)
            {
                foreach (String onename in theuser.names)
                {
                    if (onename == name)
                    {
                        return;
                    }
                }
            }
            User newUser = new User();
            newUser.AddName(name);
            TheUsers.Add(newUser);
            Flush();
        }

        public void Add(User name)
        {
            foreach (User theuser in TheUsers)
            {
                foreach (String onename in theuser.names)
                {
                    foreach (String thename in name.names)
                    {
                        if (onename == thename)
                        {
                            return;
                        }
                    }
                }
            }
            TheUsers.Add(name);
            Flush();
        }

        public void Remove(String name)
        {
            List<User> forUsers = TheUsers;
            for (int i = 0; i < forUsers.Count; i++)
            {
                foreach (String thename in forUsers[i].names)
                {
                    if (thename == name)
                    {
                        TheUsers[i] = null;
                        TheUsers.RemoveAt(i);
                        Flush();
                        return;
                    }
                }
            }
        }

        public void GroupUser(String user1, String user2)
        {
            int u1 = 0, u2 = 0;
            for (int i = 0; i < TheUsers.Count; i++)
            {
                for (int x = 0; x < TheUsers[i].names.Count; x++)
                {
                    if (TheUsers[i].names[x] == user1)
                    {
                        u1 = i;
                    }
                    if (TheUsers[i].names[x] == user2)
                    {
                        u2 = i;
                    }
                }
            }
            if (u1 == 0 || u2 == 0)
            {
                throw new Exception("User not found");
            }
            foreach (String oldname in TheUsers[u2].names)
            {
                TheUsers[u1].AddName(oldname);
            }
            foreach (String oldbox in TheUsers[u2].boxes)
            {
                TheUsers[u1].AddBox(oldbox);
            }
            foreach (String oldjoke in TheUsers[u2].jokes)
            {
                TheUsers[u1].AddJoke(oldjoke);
            }
            if (TheUsers[u2].last_seen > TheUsers[u1].last_seen)
            {
                TheUsers[u1].last_seen = TheUsers[u2].last_seen;
            }
            if (TheUsers[u2].last_messaged > TheUsers[u1].last_messaged)
            {
                TheUsers[u1].last_messaged = TheUsers[u2].last_messaged;
                TheUsers[u1].last_message = TheUsers[u2].last_message;
            }
            if (TheUsers[u2].ignored)
            {
                TheUsers[u1].ignored = true;
            }
            if (TheUsers[u2].asked)
            {
                TheUsers[u1].asked = true;
            }
            if (TheUsers[u2].is_op)
            {
                TheUsers[u1].is_op = true;
            }
            TheUsers.RemoveAt(u2);
            Flush();
        }

        public List<String> AllJokes()
        {
            List<String> TheJokes = new List<String>();
            foreach (User TheUser in TheUsers)
            {
                TheJokes.AddRange(TheUser.jokes);
            }
            return TheJokes;
        }

        public alias_db AllAliases()
        {
            alias_db TheAliases = new alias_db();
            foreach (User TheUser in TheUsers)
            {
                for (int i = 0; i < TheUser.alias.alias.Count; i++)
                {
                    TheAliases[TheUser.alias.alias[i]] = TheUser.alias.description[i];
                }
            }
            return TheAliases;
        }

        public IEnumerator GetEnumerator()
        {
            return TheUsers.GetEnumerator();
        }
    }

    public class User
    {
        public List<String> names;
        public List<String> boxes;
        public List<String> jokes;
        public alias_db alias;
        public String password;
        public DateTime last_seen;
        public DateTime last_messaged;
        public String last_message;
        public DateTime authcookiedate;
        public Boolean authenticated;
        public Boolean ignored;
        public Boolean asked;
        public Boolean is_op;

        public User()
        {
            names = new List<String>();
            boxes = new List<String>();
            jokes = new List<String>();
            alias = new alias_db();
            password = "";
            last_seen = new DateTime();
            last_messaged = new DateTime();
            last_message = "";
            authcookiedate = DateTime.MinValue;
            authenticated = false;
            ignored = false;
            asked = false;
            is_op = false;
        }

        public void SetMessage(String message)
        {
            last_messaged = DateTime.Now;
            last_message = message;
        }

        public void SetSeen()
        {
            last_seen = DateTime.Now;
        }

        public void SetPassword(String pw)
        {
            password = toolbox.crypt(pw);
        }

        public Boolean CheckPassword(String pw)
        {
            if (password == toolbox.crypt(pw))
            {
                return true;
            }
            return false;
        }

        public Boolean AddBox(String boxname)
        {
            if (!boxes.Contains(boxname))
            {
                boxes.Add(boxname);
                return true;
            }
            return false;
        }

        public Boolean AddName(String name)
        {
            if (!names.Contains(name))
            {
                names.Add(name);
                return true;
            }
            return false;
        }

        public Boolean AddJoke(String joke)
        {
            if (!jokes.Contains(joke))
            {
                jokes.Add(joke);
                return true;
            }
            return false;
        }

        public Boolean AddAlias(String alias, String description)
        {
            if (freetzbot.Program.TheUsers.AllAliases()[alias] == "")
            {
                this.alias[alias] = description;
                return true;
            }
            return false;
        }
    }

    public class ConvertOld
    {
        public static void StartConvert(UserCollection TheUsers)
        {
            ConvertUserDB(TheUsers);
            ConvertBoxDB(TheUsers);
            ConvertSeenDB(TheUsers);
            ConvertJokeDB(TheUsers);
            ConvertAliasDB(TheUsers);
        }

        public static void ConvertSeenDB(UserCollection TheUsers)
        {
            db seendb = toolbox.getDatabaseByName("seen.db");
            foreach (String data in seendb.GetAll())
            {
                String[] daten = data.Split(';');//User;Joined;Messaged;Message
                DateTime.TryParse(daten[1], out TheUsers[daten[0]].last_seen);
                DateTime.TryParse(daten[2], out TheUsers[daten[0]].last_messaged);
                if (daten[3].Contains("ACTION "))
                {
                    daten[3] = daten[3].Remove(0, 1);
                    daten[3] = daten[3].Remove(daten[3].Length - 1);
                }
                TheUsers[daten[0]].last_message = daten[3];
            }
        }

        public static void ConvertBoxDB(UserCollection TheUsers)
        {
            db boxdb = toolbox.getDatabaseByName("box.db");
            foreach (String line in boxdb.GetAll())
            {
                String[] split = line.Split(':');
                TheUsers[split[0]].AddBox(split[1]);
            }
        }

        public static void ConvertUserDB(UserCollection TheUsers)
        {
            db userdb = toolbox.getDatabaseByName("user.db");
            foreach (String name in userdb.GetAll())
            {
                User newUser = new User();
                newUser.AddName(name);
                newUser.asked = true;
                TheUsers.Add(newUser);
            }
        }

        public static void ConvertJokeDB(UserCollection TheUsers)
        {
            db witzdb = toolbox.getDatabaseByName("witze.db");
            foreach (String joke in witzdb.GetAll())
            {
                TheUsers["FritzBot"].AddJoke(joke);
            }
        }

        public static void ConvertAliasDB(UserCollection TheUsers)
        {
            db aliasdb = toolbox.getDatabaseByName("alias.db");
            foreach (String alias in aliasdb.GetAll())
            {
                String[] split = alias.Split('=');
                TheUsers["FritzBot"].alias[split[0]] = split[1];
            }
        }
    }
}
