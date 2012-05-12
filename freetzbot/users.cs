using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Xml;
using System.Text;
using System.Data;

namespace FritzBot
{
    class UserCollection : IEnumerable
    {
        private List<User> TheUsers;
        private Thread AutoFlushThread;
        
        public User this[String name]
        {
            get
            {
                if (name.Contains("#") || name.Contains("."))
                {
                    return new User();
                }
                foreach (User theuser in TheUsers)
                {
                    foreach (String onename in theuser.names)
                    {
                        if (onename.ToLower() == name.ToLower())
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
                if (int.TryParse(Program.configuration["UserAutoFlushIntervall"], out converttime))
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
                    if (onename.ToLower() == name.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Flush()
        {
            Boolean failed = false;
            do
            {
                XmlTextWriter UDB = null;
                XmlSerializer serializer = null;
                try
                {
                    TheUsers.Sort();
                    UDB = new XmlTextWriter("users.db", Encoding.GetEncoding("iso-8859-1"));
                    UDB.Formatting = Formatting.Indented;
                    serializer = new XmlSerializer(TheUsers.GetType());
                    serializer.Serialize(UDB, TheUsers);
                    UDB.Flush();
                    failed = false;
                }
                catch
                {
                    failed = true;
                    Thread.Sleep(50);
                }
                finally
                {
                    UDB.Close();
                }
            } while (failed);
        }

        public void Reload()
        {
            if (File.Exists("users.db"))
            {
                FileInfo FI = new FileInfo("users.db");
                if (FI.Length > 0)
                {
                    StreamReader UDB = null;
                    XmlSerializer serializer = null;
                    try
                    {
                        UDB = new StreamReader("users.db", Encoding.GetEncoding("iso-8859-1"));
                        serializer = new XmlSerializer(TheUsers.GetType());
                        TheUsers = (List<User>)serializer.Deserialize(UDB);
                        foreach (User oneuser in TheUsers)
                        {
                            oneuser.authenticated = false;
                            oneuser.authcookiedate = DateTime.MinValue;
                        }
                    }
                    finally
                    {
                        UDB.Close();
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

        public void Maintain()
        {
            List<User> newUsers = new List<User>(TheUsers);
            List<String> allNames = new List<String>();
            List<String> doubleNames = new List<String>();
            int UserCount = newUsers.Count;
            for (int i = 0; i < UserCount; i++)
            {
                int NamesCount = newUsers[i].names.Count;
                for (int x = 0; x < NamesCount; x++)
                {
                    if (newUsers[i].names[x].Contains(".") || newUsers[i].names[x].Contains("#") || String.IsNullOrEmpty(newUsers[i].names[x]))
                    {
                        newUsers[i].names.RemoveAt(x);
                        x--;
                        NamesCount--;
                    }
                    else if (allNames.Contains(newUsers[i].names[x]))
                    {
                        doubleNames.Add(newUsers[i].names[x]);
                    }
                    else
                    {
                        allNames.Add(newUsers[i].names[x]);
                    }
                }
                if (newUsers[i].names.Count == 0)
                {
                    newUsers.RemoveAt(i);
                    i--;
                    UserCount--;
                }
            }
            newUsers.Sort();
            TheUsers.Clear();
            TheUsers = null;
            TheUsers = newUsers;
        }

        public void CleanUp()
        {
            List<User> newUsers = new List<User>();
            foreach (User oneuser in TheUsers)
            {
                if (oneuser.last_seen > DateTime.Today.AddMonths(-6) || oneuser.boxes.Count > 0)
                {
                    newUsers.Add(oneuser);
                }
            }
            newUsers.Sort();
            TheUsers.Clear();
            TheUsers = null;
            TheUsers = newUsers;
        }

        public void GroupUser(String user1, String user2)
        {
            User Fusioned = new User();
            int u1 = -1, u2 = -1;
            for (int i = 0; i < TheUsers.Count; i++)
            {
                for (int x = 0; x < TheUsers[i].names.Count; x++)
                {
                    if (TheUsers[i].names[x] == user1 && u2 == -1)
                    {
                        Fusioned = TheUsers[i];
                        u2 = i;
                        break;
                    }
                    if (TheUsers[i].names[x] == user2 && u1 == -1)
                    {
                        u1 = i;
                        break;
                    }
                }
            }
            if (u1 == -1 || u2 == -1)
            {
                throw new ArgumentException("User not found");
            }
            foreach (String oldname in TheUsers[u1].names)
            {
                Fusioned.AddName(oldname);
            }
            foreach (String oldbox in TheUsers[u1].boxes)
            {
                Fusioned.AddBox(oldbox);
            }
            foreach (String oldjoke in TheUsers[u1].jokes)
            {
                Fusioned.AddJoke(oldjoke);
            }
            if (TheUsers[u1].last_seen > Fusioned.last_seen)
            {
                Fusioned.last_seen = TheUsers[u1].last_seen;
            }
            if (TheUsers[u1].last_messaged > Fusioned.last_messaged)
            {
                Fusioned.last_messaged = TheUsers[u1].last_messaged;
                Fusioned.last_message = TheUsers[u1].last_message;
            }
            if (TheUsers[u1].ignored)
            {
                Fusioned.ignored = true;
            }
            if (TheUsers[u1].asked)
            {
                Fusioned.asked = true;
            }
            if (TheUsers[u1].isOp)
            {
                Fusioned.isOp = true;
            }
            TheUsers.RemoveAt(u1);
            TheUsers.RemoveAt(u2);
            TheUsers.Add(Fusioned);
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

        public AliasDB AllAliases()
        {
            AliasDB TheAliases = new AliasDB();
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

    public class User : IComparable
    {
        public List<String> names;
        public List<String> boxes;
        public List<String> jokes;
        public AliasDB alias;
        public String password;
        public DateTime last_seen;
        public DateTime last_messaged;
        public String last_message;
        public DateTime authcookiedate;
        public Boolean authenticated;
        public Boolean ignored;
        public Boolean asked;
        public Boolean isOp;
        public List<String> RememberNick;
        public List<String> RememberMessage;
        public List<DateTime> RememberTime;
        //public List<DateTime> RememberInTime;
        public List<Boolean> Remembered;

        public User()
        {
            names = new List<String>();
            boxes = new List<String>();
            jokes = new List<String>();
            alias = new AliasDB();
            password = "";
            last_seen = new DateTime();
            last_messaged = new DateTime();
            last_message = "";
            authcookiedate = DateTime.MinValue;
            authenticated = false;
            ignored = false;
            asked = false;
            isOp = false;
            RememberNick = new List<String>();
            RememberMessage = new List<String>();
            RememberTime = new List<DateTime>();
            //RememberInTime = new List<DateTime>();
            Remembered = new List<Boolean>();
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
            password = toolbox.Crypt(pw);
        }

        public Boolean CheckPassword(String pw)
        {
            if (password == toolbox.Crypt(pw))
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

        public Boolean AddAlias(String theAlias, String description)
        {
            if (String.IsNullOrEmpty(Program.TheUsers.AllAliases()[theAlias]))
            {
                alias[theAlias] = description;
                return true;
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            User CompareUser = (User)obj;
            return names[0].CompareTo(CompareUser.names[0]);
        }

        public void AddRemember(String nick, String toRemember)
        {
            AddRemember(nick, toRemember, DateTime.MinValue);
        }

        public void AddRemember(String nick, String toRemember, DateTime RememberIn)
        {
            RememberNick.Add(nick);
            RememberMessage.Add(toRemember);
            RememberTime.Add(DateTime.Now);
            //RememberInTime.Add(RememberIn);
            Remembered.Add(false);
        }

        public List<String[]> GetUnreadRemembers()
        {
            List<String[]> UnreadRemembers = new List<String[]>();
            while (Remembered.Count > 0)
            {
                int i = 0;
                if (!Remembered[i])
                {
                    String[] TheRemember = new String[4];
                    TheRemember[0] = RememberNick[i];
                    TheRemember[1] = RememberMessage[i];
                    TheRemember[2] = RememberTime[i].ToString("dd.MM.yyyyTHH:mm:ss");
                    //TheRemember[3] = RememberInTime[i].ToString("dd.MM.yyyyTHH:mm:ss");
                    RememberNick.RemoveAt(i);
                    RememberMessage.RemoveAt(i);
                    RememberTime.RemoveAt(i);
                    Remembered.RemoveAt(i);
                    //RememberInTime.RemoveAt(i);
                    UnreadRemembers.Add(TheRemember);
                }
                else
                {
                    i++;
                }
            }
            return UnreadRemembers;
        }

        public List<String[]> GetAllRemembers()
        {
            List<String[]> Remembers = new List<String[]>();
            while (RememberMessage.Count > 0)
            {
                String[] TheRemember = new String[4];
                TheRemember[0] = RememberNick[0];
                TheRemember[1] = RememberMessage[0];
                TheRemember[2] = RememberTime[0].ToString("dd.MM.yyyyTHH:mm:ss");
                //TheRemember[3] = RememberInTime[0].ToString("dd.MM.yyyyTHH:mm:ss");
                RememberNick.RemoveAt(0);
                RememberMessage.RemoveAt(0);
                RememberTime.RemoveAt(0);
                Remembered.RemoveAt(0);
                //RememberInTime.RemoveAt(0);
                Remembers.Add(TheRemember);
            }
            return Remembers;
        }
    }

    static class ConvertOld
    {
        public static void StartConvert(UserCollection TheUsers)
        {
            ConvertUserDB(TheUsers);
            ConvertBoxDB(TheUsers);
            ConvertSeenDB(TheUsers);
            ConvertJokeDB(TheUsers);
            ConvertAliasDB(TheUsers);
            TheUsers.Flush();
        }

        public static void ConvertSeenDB(UserCollection theUsers)
        {
            db seendb = toolbox.getDatabaseByName("seen.db");
            foreach (String data in seendb.GetAll())
            {
                String[] daten = data.Split(';');//User;Joined;Messaged;Message
                if (!DateTime.TryParse(daten[1], out theUsers[daten[0]].last_seen))
                {
                    theUsers[daten[0]].last_seen = DateTime.MinValue;
                }
                if (!DateTime.TryParse(daten[2], out theUsers[daten[0]].last_messaged))
                {
                    theUsers[daten[0]].last_messaged = DateTime.MinValue;
                }
                if (daten[3].Contains("ACTION "))
                {
                    daten[3] = daten[3].Remove(0, 1);
                    daten[3] = daten[3].Remove(daten[3].Length - 1);
                }
                theUsers[daten[0]].last_message = daten[3];
            }
        }

        public static void ConvertBoxDB(UserCollection theUsers)
        {
            db boxdb = toolbox.getDatabaseByName("box.db");
            foreach (String line in boxdb.GetAll())
            {
                String[] split = line.Split(':');
                theUsers[split[0]].AddBox(split[1]);
            }
        }

        public static void ConvertUserDB(UserCollection theUsers)
        {
            db userdb = toolbox.getDatabaseByName("user.db");
            foreach (String name in userdb.GetAll())
            {
                User newUser = new User();
                newUser.AddName(name);
                newUser.asked = true;
                theUsers.Add(newUser);
            }
        }

        public static void ConvertJokeDB(UserCollection theUsers)
        {
            db witzdb = toolbox.getDatabaseByName("witze.db");
            foreach (String joke in witzdb.GetAll())
            {
                theUsers["FritzBot"].AddJoke(joke);
            }
        }

        public static void ConvertAliasDB(UserCollection theUsers)
        {
            db aliasdb = toolbox.getDatabaseByName("alias.db");
            foreach (String alias in aliasdb.GetAll())
            {
                String[] split = alias.Split('=');
                theUsers["FritzBot"].alias[split[0]] = split[1];
            }
        }
    }
}