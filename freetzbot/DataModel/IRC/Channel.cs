using FritzBot.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace FritzBot.DataModel.IRC
{
    public class Channel
    {
        private Irc Connection;
        public List<User> User { get; private set; }
        public string ChannelName { get; private set; }
        public bool EndOfWho { get; set; }

        public Channel(Irc irc, string name)
        {
            ChannelName = name;
            Connection = irc;
            User = new List<User>();
            RefreshUser();
        }

        public void RefreshUser()
        {
            Connection.Sendraw("WHO " + ChannelName);
        }

        public void SendMessage(string Message)
        {
            Connection.Sendmsg(Message, ChannelName);
        }

        public override string ToString()
        {
            return ChannelName;
        }
    }
}