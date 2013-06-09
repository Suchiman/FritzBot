using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FritzBot.Functions
{
    public class Url
    {
        private string _protokoll = String.Empty;
        private string _host = String.Empty;
        private string _path = String.Empty;
        private Dictionary<string, string> _params = new Dictionary<string, string>();

        public Url(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url");
            }
            if (url.Contains("://"))
            {
                _protokoll = url.Substring(0, url.IndexOf(':'));
                url = url.Remove(0, _protokoll.Length + 3);
            }
            if (url.Contains("/"))
            {
                _host = url.Substring(0, url.IndexOf('/'));
                url = url.Remove(0, url.IndexOf('/'));

                if (url.Contains('?'))
                {
                    _path = url.Substring(0, url.IndexOf('?'));
                    url = url.Remove(0, url.IndexOf('?') + 1);

                    string[] undsplit = url.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string split in undsplit)
                    {
                        string[] gleichsplit = split.Split(new[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string key = String.Empty, value = String.Empty;

                        if (gleichsplit.Length == 0)
                        {
                            continue;
                        }

                        key = HttpUtility.UrlDecode(gleichsplit[0]);

                        if (gleichsplit.Length > 1)
                        {
                            value = HttpUtility.UrlDecode(gleichsplit[1]);
                        }
                        _params[key] = value;
                    }
                }
                else
                {
                    _path = url;
                }
            }
            else
            {
                _host = url;
            }
        }

        public bool TryGetParameter(string key, out string value)
        {
            return _params.TryGetValue(key, out value);
        }

        public string GetParameter(string key)
        {
            if (!_params.ContainsKey(key))
            {
                throw new Exception("Diesen Schlüssel gibt es nicht");
            }
            return _params[key];
        }

        public void SetParameter(string key, string value)
        {
            _params[key] = value;
        }

        public void RemoveParameter(string key)
        {
            if (_params.ContainsKey(key))
            {
                _params.Remove(key);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(_protokoll))
            {
                sb.Append(_protokoll + "://");
            }
            sb.Append(_host);
            sb.Append(_path);
            if (_params.Count > 0)
            {
                sb.Append("?");
                sb.Append(String.Join("&", _params.Select(x => String.Format("{0}={1}", HttpUtility.UrlEncode(x.Key), HttpUtility.UrlEncode(x.Value)))));
            }
            return sb.ToString();
        }
    }
}