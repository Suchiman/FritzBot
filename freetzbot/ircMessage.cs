using FritzBot.Core;
using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot
{
    /// <summary>
    /// Representiert eine einzelne IRC Nachricht
    /// </summary>
    public class ircMessage
    {
        /// <summary>
        /// Erstellt ein neues ircMessage Objekt um eine IRC Nachricht dazustellen
        /// </summary>
        /// <param name="data">Die Ausgangsdaten</param>
        /// <param name="server">Der Server von dem die Nachricht kommt</param>
        /// <param name="user">Der der Nachricht zugeordnete User</param>
        public ircMessage(IrcMessageData data, Server server, User user)
        {
            Data = data;
            Server = server;
            TheUser = user;
            Nickname = Data.Nick;
            Source = Data.Channel ?? Data.Nick;
            Answered = false;
            UnloggedMessages = new Queue<string>(3);
            List<string> MessageTmp = Data.MessageArray.Where(x => !String.IsNullOrEmpty(x)).ToList();
            Message = String.Join(" ", MessageTmp);
            if (TheUser == null || (Nickname.Contains("#") || Nickname.Contains(".") || Server.IrcClient.IsMe(Nickname) || Nickname.ToLower().Contains("nickserv") || TheUser.Ignored || Data.Message.Contains("[Global Notice]")) && !TheUser.Admin)
            {
                IsIgnored = true;
            }
            else
            {
                IsIgnored = false;
            }
            IsPrivate = data.Type.In(ReceiveType.QueryAction, ReceiveType.QueryMessage, ReceiveType.QueryNotice);
            if (Message.Length > 0)
            {
                IsCommand = Message[0] == '!';
            }
            else
            {
                IsCommand = false;
            }
            if (IsCommand)
            {
                CommandArgs = MessageTmp.Skip(1).ToList();
                CommandLine = String.Join(" ", CommandArgs);
                CommandName = MessageTmp.First().TrimStart('!');
            }
            else
            {
                CommandArgs = MessageTmp;
                CommandLine = Message;
                CommandName = "";
            }
            HasArgs = CommandArgs.Count > 0;
        }

        /// <summary>
        /// Der Nickname von dem diese Nachricht stammt
        /// </summary>
        public string Nickname { get; protected set; }

        /// <summary>
        /// Die SmartIrc4net ausgangsdaten
        /// </summary>
        public IrcMessageData Data { get; protected set; }

        /// <summary>
        /// Der Server von dem diese Nachricht stammt
        /// </summary>
        public Server Server { get; protected set; }

        /// <summary>
        /// Gibt an, ob die Nachricht per QUERY gesandt wurde
        /// </summary>
        public bool IsPrivate { get; protected set; }

        /// <summary>
        /// Gibt an ob diese Nachricht zu ignorieren ist
        /// </summary>
        public bool IsIgnored { get; protected set; }

        /// <summary>
        /// Gibt an ob die Nachricht ein Befehl ist ( ! Befehlprefix )
        /// </summary>
        public bool IsCommand { get; protected set; }

        /// <summary>
        /// Gibt an, ob es weitere CommandArgs gibt
        /// </summary>
        public bool HasArgs { get; protected set; }

        /// <summary>
        /// Gibt an ob dem Benutzer im IRC bereits geantwortet wurde
        /// </summary>
        public bool Answered { get; protected set; }

        /// <summary>
        /// Gibt an, ob die Nachricht unabhängig vom Ursprung Privat gesendet wird
        /// </summary>
        public bool ForcedPrivat
        {
            get
            {
                return ConfigHelper.GetBoolean("Silence", false);
            }
        }

        /// <summary>
        /// Gibt an, ob diese Nachricht von einem Command behandelt wurde
        /// </summary>
        public bool ProcessedByCommand { get; set; }

        /// <summary>
        /// Gibt an, ob die Nachricht von einem Event verarbeitet wurde und nicht mit einem Command bearbeitet werden soll
        /// </summary>
        public bool HandledByEvent { get; set; }

        /// <summary>
        /// Gibt an, ob die Nachricht nicht geloggt werden soll
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Wenn es sich um einen Befehl handelt, stellt dies den Befehlsnamen (!befehlsname) da
        /// </summary>
        public string CommandName { get; protected set; }

        /// <summary>
        /// Gibt alles zurück, was hinter dem Befehlsnamen steht
        /// </summary>
        public string CommandLine { get; protected set; }

        /// <summary>
        /// Enthält die Leerzeichen getrennte Befehlsargumente
        /// </summary>
        public List<string> CommandArgs { get; protected set; }

        /// <summary>
        /// Gibt den User von dem die Nachricht stammt zurück
        /// </summary>
        public User TheUser { get; protected set; }

        /// <summary>
        /// Die bereinigte IRC Nachricht
        /// </summary>
        public string Message { get; protected set; }

        /// <summary>
        /// Die ursprüngliche IRC Nachricht
        /// </summary>
        public string MessageRaw
        {
            get
            {
                return Data.Message;
            }
        }

        /// <summary>
        /// Bisher ungeloggte IRC Nachrichten
        /// </summary>
        public Queue<string> UnloggedMessages { get; protected set; }

        /// <summary>
        /// Die Quelle der Nachricht (#channel oder Nickname)
        /// </summary>
        public string Source { get; protected set; }

        /// <summary>
        /// Sendet dem Nutzer die Hilfe
        /// </summary>
        public void AnswerHelp(object plugin)
        {
            Module.HelpAttribute help = toolbox.GetAttribute<Module.HelpAttribute>(plugin);
            Answer(help.Help);
        }

        /// <summary>
        /// Antwortet dem Benutzer dort wo er den Befehl aufgerufen hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void Answer(string Message)
        {
            Answered = true;
            Server.IrcClient.SendMessage(SendType.Message, Source, Message);
            UnloggedMessages.Enqueue(String.Format("An {0}: {1}", ForcedPrivat ? Nickname : Source, Message));
        }

        /// <summary>
        /// Sendet dem Benutzer dort wo er den Befehl aufgerufen eine Aktion hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void AnswerAction(string Message)
        {
            Answered = true;
            Server.IrcClient.SendMessage(SendType.Action, Source, Message);
            UnloggedMessages.Enqueue(String.Format("An {0}: ***{1}***", ForcedPrivat ? Nickname : Source, Message));
        }

        /// <summary>
        /// Schickt dem Benutzer eine Nachricht im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateMessage(string Message)
        {
            Answered = true;
            Server.IrcClient.SendMessage(SendType.Message, Nickname, Message);
            UnloggedMessages.Enqueue(String.Format("An {0}: {1}", Nickname, Message));
        }

        /// <summary>
        /// Schickt dem Benutzer eine Aktion im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateAction(string Message)
        {
            Answered = true;
            Server.IrcClient.SendMessage(SendType.Action, Nickname, Message);
            UnloggedMessages.Enqueue(String.Format("An {0}: {1}", Nickname, Message));
        }

        public override string ToString()
        {
            return Message;
        }
    }
}