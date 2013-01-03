using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzBot.DataModel.IRC
{
    public abstract class IRCEvent
    {
        public Irc IRC { get; set; }
        public String Nickname { get; set; }
        public IRCEvent(Irc obj)
        {
            IRC = obj;
        }
    }

    public class Join : IRCEvent
    {
        public Channel Channel { get; set; }
        public Join(Irc obj) : base(obj) { }
        public override string ToString()
        {
            return String.Format("{0} hat den Raum {1} betreten", Nickname, Channel.ChannelName);
        }
    }

    public class Quit : IRCEvent
    {
        public Quit(Irc obj) : base(obj) { }
        public override string ToString()
        {
            return String.Format("{0} hat den Server verlassen", Nickname);
        }
    }

    public class Part : IRCEvent
    {
        public Channel Channel { get; set; }
        public Part(Irc obj) : base(obj) { }
        public override string ToString()
        {
            return String.Format("{0} hat den Raum {1} verlassen", Nickname, Channel.ChannelName);
        }
    }

    public class Kick : IRCEvent
    {
        public String KickedBy { get; set; }
        public Channel Channel { get; set; }
        public Kick(Irc obj) : base(obj) { }
        public override string ToString()
        {
            return String.Format("{0} wurde von {1} aus dem Raum {2} gekickt", Nickname, KickedBy, Channel.ChannelName);
        }
    }

    public class Nick : IRCEvent
    {
        public String NewNickname { get; set; }
        public Nick(Irc obj) : base(obj) { }
        public override string ToString()
        {
            return String.Format("{0} heißt jetzt {1}", Nickname, NewNickname);
        }
    }
}