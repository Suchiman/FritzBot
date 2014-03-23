using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.Core
{
    public class User : IComparable
    {
        public List<string> Names { get; set; }
        public string LastUsedName { get; set; }
        public string Password { get; set; }
        public DateTime Authentication { get; set; }
        public bool Authenticated
        {
            get
            {
                return Authentication > DateTime.Now.AddDays(-1);
            }
        }
        public bool Ignored { get; set; }
        public bool Admin { get; set; }
        public bool Online
        {
            get
            {
                return ServerManager.GetInstance().Any(x => x.IrcClient != null && x.IrcClient.GetChannels().Select(c => x.IrcClient.GetChannel(c)).Any(c => c.Users.Keys.OfType<string>().Any(cn => Names.Contains(cn))));
            }
        }

        public User()
        {
            Names = new List<string>();
        }

        public void SetPassword(string pw)
        {
            Password = toolbox.Crypt(pw);
        }

        public bool CheckPassword(string pw)
        {
            if (Password == toolbox.Crypt(pw))
            {
                return true;
            }
            return false;
        }

        public bool AddName(string name)
        {
            if (!Names.Contains(name))
            {
                Names.Add(name);
                return true;
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            User CompareUser = obj as User;
            return LastUsedName.CompareTo(CompareUser.LastUsedName);
        }

        public override string ToString()
        {
            return LastUsedName;
        }
    }
}