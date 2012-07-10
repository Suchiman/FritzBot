using System;
using System.Collections.Generic;

namespace FritzBot
{
    public class ircMessage
    {
        private String _sender;
        private String _source;
        private String _message;
        private Boolean _isprivate;
        private Boolean _iscommand;
        private Boolean _answered;
        private Boolean _hasargs;
        private Boolean _handled;
        private Boolean _preprocessed;
        private Boolean _hidden;
        private Boolean _ignored;
        private String _commandline;
        private String _commandname;
        private List<String> _args;
        private User _theuser;
        private UserCollection _theusers;
        private Irc _connection;

        /// <summary>
        /// Erstellt ein neues ircMessage Objekt um eine IRC Nachricht dazustellen
        /// </summary>
        /// <param name="sender">Der Sender der Nachricht</param>
        /// <param name="source">Die Quelle (entweder der Channel oder der Nickname)</param>
        /// <param name="message">Die Nachricht</param>
        /// <param name="AllUsers">Die Benutzerdatenbank</param>
        /// <param name="connection">Die Zugrunde liegende IRC Verbindung</param>
        public ircMessage(String sender, String source, String message, UserCollection AllUsers, Irc connection)
        {
            _sender = sender;
            _source = source;
            _message = message;
            _theusers = AllUsers;
            _connection = connection;
            _theuser = _theusers[_sender];
            _answered = false;
            if ((_sender.Contains("#") || _sender.Contains(".") || _sender.Contains(_connection.Nickname) || _sender.ToLower().Contains("nickserv") || _theuser.ignored || _message.Contains("[Global Notice]")) && !_theuser.IsOp)
            {
                _ignored = true;
            }
            else
            {
                _ignored = false;
            }
            _isprivate = _sender == _source;
            if (message.Length > 0)
            {
                _iscommand = message.ToCharArray()[0] == '!';
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
                _args = new List<String>(_commandline.Split(' '));
            }
            else
            {
                _commandline = "";
                _commandname = "";
                _args = new List<String>();
            }
            _hasargs = !String.IsNullOrEmpty(_commandline);
        }
        /// <summary>
        /// Gibt an, ob die Nachricht per QUERY gesandt wurde
        /// </summary>
        public Boolean IsPrivate
        {
            get
            {
                return _isprivate;
            }
        }
        /// <summary>
        /// Gibt an ob diese Nachricht zu ignorieren ist
        /// </summary>
        public Boolean IsIgnored
        {
            get
            {
                return _ignored;
            }
        }
        /// <summary>
        /// Gibt an ob die Nachricht ein Befehl ist ( ! Befehlprefix )
        /// </summary>
        public Boolean IsCommand
        {
            get
            {
                return _iscommand;
            }
        }
        /// <summary>
        /// Gibt an, wenn es sich um einen Befehl handelt, ob weitere Argumente mitgeschickt wurden
        /// </summary>
        public Boolean HasArgs
        {
            get
            {
                return _hasargs;
            }
        }
        /// <summary>
        /// Gibt an ob dem Benutzer im IRC geantwortet wurde
        /// </summary>
        public Boolean Answered
        {
            get
            {
                return _answered;
            }
        }
        /// <summary>
        /// Gibt an, ob die Nachricht durch ein Event verarbeitet wurde und von der Restlichen Verarbeitung ausgeschlossen wird
        /// </summary>
        public Boolean Handled
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
        /// Gibt an ob die Nachricht das MessageEvent durchlaufen hat
        /// </summary>
        public Boolean PreProcessed
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
        public Boolean Hidden
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
        public String CommandName
        {
            get
            {
                return _commandname;
            }
        }
        /// <summary>
        /// Gibt alles zurück, was hinter dem Befehlsnamen steht
        /// </summary>
        public String CommandLine
        {
            get
            {
                return _commandline;
            }
        }
        /// <summary>
        /// Enthält die Leerzeichen getrennte Befehlsargumente
        /// </summary>
        public List<String> CommandArgs
        {
            get
            {
                return _args;
            }
        }
        /// <summary>
        /// Stellt den IRC Namen des Benutzers da
        /// </summary>
        public String Nick
        {
            get
            {
                return _sender;
            }
        }
        /// <summary>
        /// Gibt den User von dem die Nachricht stammt zurück
        /// </summary>
        public User TheUser
        {
            get
            {
                return _theuser;
            }
        }
        /// <summary>
        /// Die Benutzerdatenbank
        /// </summary>
        public UserCollection TheUsers
        {
            get
            {
                return _theusers;
            }
        }
        /// <summary>
        /// Die ursprüngliche IRC Nachricht
        /// </summary>
        public String Message
        {
            get
            {
                return _message;
            }
        }
        /// <summary>
        /// Die Quelle der Nachricht (#channel oder Nickname)
        /// </summary>
        public String Source
        {
            get
            {
                return _source;
            }
        }
        /// <summary>
        /// Antwortet dem Benutzer dort wo er den Befehl aufgerufen hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void Answer(String Message)
        {
            _answered = true;
            _connection.Sendmsg(Message, _source);
        }
        /// <summary>
        /// Sendet dem Benutzer dort wo er den Befehl aufgerufen eine Aktion hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void AnswerAction(String Message)
        {
            _answered = true;
            _connection.Sendaction(Message, _source);
        }
        /// <summary>
        /// Schickt dem Benutzer eine Nachricht im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateMessage(String Message)
        {
            _answered = true;
            _connection.Sendmsg(Message, _sender);
        }
        /// <summary>
        /// Schickt dem Benutzer eine Aktion im QUERY, unabhängig davon wo er geschrieben hat
        /// </summary>
        /// <param name="Message">Den zu Antwortenden Text</param>
        public void SendPrivateAction(String Message)
        {
            _answered = true;
            _connection.Sendaction(Message, _sender);
        }
        /// <summary>
        /// Gibt die zugrunde liegende IRC Verbindung zurück
        /// </summary>
        public Irc Connection
        {
            get
            {
                return _connection;
            }
        }
    }
}