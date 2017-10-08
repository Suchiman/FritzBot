using FritzBot.Core;
using FritzBot.Plugins;
using Meebey.SmartIrc4net;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FritzBot
{
    /// <summary>
    /// Representiert eine einzelne IRC Nachricht
    /// </summary>
    public class IrcMessage
    {
        /// <summary>
        /// Erstellt ein neues ircMessage Objekt um eine IRC Nachricht dazustellen
        /// </summary>
        /// <param name="data">Die Ausgangsdaten</param>
        /// <param name="serverConnetion">Die ServerConnetion von dem die Nachricht kommt</param>
        public IrcMessage(IrcMessageData data, ServerConnection serverConnetion)
        {
            Contract.Requires(data != null && serverConnetion != null);

            Data = data;
            ServerConnetion = serverConnetion;
            Nickname = Data.Nick;
            Source = Data.Channel ?? Data.Nick;
            Answered = false;
            UnloggedMessages = new Queue<LogEvent>(3);
            List<string> MessageTmp = Data.MessageArray.Where(x => !String.IsNullOrEmpty(x)).ToList();
            Message = MessageTmp.Join(" ");
            if (Nickname.Contains("#") || Nickname.Contains(".") || ServerConnetion.IrcClient.IsMe(Nickname) || Nickname.ToLower().Contains("nickserv") || Data.Message.Contains("[Global Notice]"))
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
                CommandLine = CommandArgs.Join(" ");
                CommandName = MessageTmp[0].TrimStart('!');
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
        /// Die ServerConnetion von dem diese Nachricht stammt
        /// </summary>
        public ServerConnection ServerConnetion { get; protected set; }

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
        public Queue<LogEvent> UnloggedMessages { get; protected set; }

        /// <summary>
        /// Die Quelle der Nachricht (#channel oder Nickname)
        /// </summary>
        public string Source { get; protected set; }

        /// <summary>
        /// Sendet dem Nutzer die Hilfe
        /// </summary>
        public void AnswerHelp(object plugin)
        {
            Contract.Requires(plugin != null);

            HelpAttribute help = toolbox.GetAttribute<HelpAttribute>(plugin);
            if (help == null)
            {
                throw new ArgumentException("Das Plugin verfügt über keine Hilfe");
            }
            Answer(help.Help);
        }

        /// <summary>
        /// Antwortet dem Benutzer dort wo er den Befehl aufgerufen hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void Answer(string Message)
        {
            Answered = true;
            ServerConnetion.IrcClient.SendMessage(SendType.Message, Source, Message);
            UnloggedMessages.Enqueue(SerilogHack.CreateLogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, "An {Receiver:l}: {Message:l}", ForcedPrivat ? Nickname : Source, Message));
        }

        /// <summary>
        /// Sendet dem Benutzer dort wo er den Befehl aufgerufen eine Aktion hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void AnswerAction(string Message)
        {
            Answered = true;
            ServerConnetion.IrcClient.SendMessage(SendType.Action, Source, Message);
            UnloggedMessages.Enqueue(SerilogHack.CreateLogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, "An {Receiver:l}: ***{Message:l}***", ForcedPrivat ? Nickname : Source, Message));
        }

        /// <summary>
        /// Schickt dem Benutzer eine Nachricht im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateMessage(string Message)
        {
            Answered = true;
            ServerConnetion.IrcClient.SendMessage(SendType.Message, Nickname, Message);
            UnloggedMessages.Enqueue(SerilogHack.CreateLogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, "An {Receiver:l}: {Message:l}", Nickname, Message));
        }

        /// <summary>
        /// Schickt dem Benutzer eine Aktion im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateAction(string Message)
        {
            Answered = true;
            ServerConnetion.IrcClient.SendMessage(SendType.Action, Nickname, Message);
            UnloggedMessages.Enqueue(SerilogHack.CreateLogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, "An {Receiver:l}: ***{Message:l}***", Nickname, Message));
        }

        public override string ToString()
        {
            return Message;
        }
    }
}