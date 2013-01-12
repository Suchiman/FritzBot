using FritzBot.Core;
using FritzBot.DataModel.IRC;
using System;
using System.Collections.Generic;

namespace FritzBot
{
    public class ircMessage : IRCEvent
    {
        private string _origin;
        private string _message;
        private bool _isprivate;
        private bool _iscommand;
        private bool _answered;
        private bool _hasargs;
        private bool _handled;
        private bool _preprocessed;
        private bool _hidden;
        private bool _ignored;
        private string _commandline;
        private string _commandname;
        private List<string> _args;
        private Queue<string> _loggingQueue;

        /// <summary>
        /// Erstellt ein neues ircMessage Objekt um eine IRC Nachricht dazustellen
        /// </summary>
        /// <param name="sender">Der Sender der Nachricht</param>
        /// <param name="source">Die Quelle (entweder der Channel oder der Nickname)</param>
        /// <param name="message">Die Nachricht</param>
        /// <param name="AllUsers">Die Benutzerdatenbank</param>
        /// <param name="connection">Die Zugrunde liegende IRC Verbindung</param>
        public ircMessage(string sender, string source, string message, Irc connection)
            : base(connection)
        {
            Nickname = sender;
            if (source == IRC.Nickname)
            {
                source = Nickname;
            }
            _origin = source;
            _message = message.Trim();
            _answered = false;
            _loggingQueue = new Queue<string>(3);
            if ((Nickname.Contains("#") || Nickname.Contains(".") || Nickname.Contains(this.IRC.Nickname) || Nickname.ToLower().Contains("nickserv") || TheUser.ignored || _message.Contains("[Global Notice]")) && !TheUser.IsOp)
            {
                _ignored = true;
            }
            else
            {
                _ignored = false;
            }
            _isprivate = Nickname == _origin;
            if (_message.Length > 0)
            {
                _iscommand = _message.ToCharArray()[0] == '!';
            }
            else
            {
                _iscommand = false;
            }
            if (_iscommand)
            {
                int index = _message.IndexOf(' ');
                if (index == -1)
                {
                    index = _message.Length;
                }
                _commandline = _message.Remove(0, index).Trim();
                _commandname = _message.Substring(1, index - 1).Trim();
                _args = new List<string>(_commandline.Split(' '));
            }
            else
            {
                _commandline = "";
                _commandname = "";
                _args = new List<string>();
            }
            _hasargs = !String.IsNullOrEmpty(_commandline);
        }
        /// <summary>
        /// Gibt an, ob die Nachricht per QUERY gesandt wurde
        /// </summary>
        public bool IsPrivate
        {
            get
            {
                return _isprivate;
            }
        }
        /// <summary>
        /// Gibt an ob diese Nachricht zu ignorieren ist
        /// </summary>
        public bool IsIgnored
        {
            get
            {
                return _ignored;
            }
        }
        /// <summary>
        /// Gibt an ob die Nachricht ein Befehl ist ( ! Befehlprefix )
        /// </summary>
        public bool IsCommand
        {
            get
            {
                return _iscommand;
            }
        }
        /// <summary>
        /// Gibt an, wenn es sich um einen Befehl handelt, ob weitere Argumente mitgeschickt wurden
        /// </summary>
        public bool HasArgs
        {
            get
            {
                return _hasargs;
            }
        }
        /// <summary>
        /// Gibt an ob dem Benutzer im IRC geantwortet wurde
        /// </summary>
        public bool Answered
        {
            get
            {
                return _answered;
            }
        }
        /// <summary>
        /// Gibt an, ob die Nachricht durch ein Event verarbeitet wurde und von der Restlichen Verarbeitung ausgeschlossen wird
        /// </summary>
        public bool Handled
        {
            get
            {
                return _handled;
            }
            set
            {
                _handled = value;
            }
        }
        /// <summary>
        /// Gibt an, ob die Nachricht unabhängig vom Ursprung Privat gesendet wird
        /// </summary>
        public bool ForcedPrivat
        {
            get
            {
                return XMLStorageEngine.GetManager().GetGlobalSettingsStorage("Bot").GetVariable("Silence", "false") == "true";
            }
        }
        /// <summary>
        /// Gibt an ob die Nachricht das MessageEvent durchlaufen hat
        /// </summary>
        public bool PreProcessed
        {
            get
            {
                return _preprocessed;
            }
            set
            {
                _preprocessed = value;
            }
        }
        /// <summary>
        /// Gibt an ob die Nachricht nicht geloggt werden soll
        /// </summary>
        public bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
            }
        }
        /// <summary>
        /// Wenn es sich um einen Befehl handelt, stellt dies den Befehlsnamen (!befehlsname) da
        /// </summary>
        public string CommandName
        {
            get
            {
                return _commandname;
            }
        }
        /// <summary>
        /// Gibt alles zurück, was hinter dem Befehlsnamen steht
        /// </summary>
        public string CommandLine
        {
            get
            {
                return _commandline;
            }
        }
        /// <summary>
        /// Enthält die Leerzeichen getrennte Befehlsargumente
        /// </summary>
        public List<string> CommandArgs
        {
            get
            {
                return _args;
            }
        }
        /// <summary>
        /// Gibt den User von dem die Nachricht stammt zurück
        /// </summary>
        public User TheUser
        {
            get
            {
                return UserManager.GetInstance()[Nickname];
            }
        }
        /// <summary>
        /// Die ursprüngliche IRC Nachricht
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
        }
        /// <summary>
        /// Bisher ungeloggte IRC Nachrichten
        /// </summary>
        public Queue<string> UnloggedMessages
        {
            get
            {
                return _loggingQueue;
            }
        }
        /// <summary>
        /// Die Quelle der Nachricht (#channel oder Nickname)
        /// </summary>
        public string Source
        {
            get
            {
                return _origin;
            }
        }
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
            _answered = true;
            IRC.Sendmsg(Message, _origin);
            _loggingQueue.Enqueue(String.Format("An {0}: {1}", ForcedPrivat ? Nickname : _origin, Message));
        }
        /// <summary>
        /// Sendet dem Benutzer dort wo er den Befehl aufgerufen eine Aktion hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void AnswerAction(string Message)
        {
            _answered = true;
            IRC.Sendaction(Message, _origin);
            _loggingQueue.Enqueue(String.Format("An {0}: ***{1}***", ForcedPrivat ? Nickname : _origin, Message));
        }
        /// <summary>
        /// Schickt dem Benutzer eine Nachricht im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateMessage(string Message)
        {
            _answered = true;
            IRC.Sendmsg(Message, Nickname);
            _loggingQueue.Enqueue(String.Format("An {0}: {1}", Nickname, Message));
        }
        /// <summary>
        /// Schickt dem Benutzer eine Aktion im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateAction(string Message)
        {
            _answered = true;
            IRC.Sendaction(Message, Nickname);
            _loggingQueue.Enqueue(String.Format("An {0}: {1}", Nickname, Message));
        }

        public override string ToString()
        {
            return _message;
        }
    }
}