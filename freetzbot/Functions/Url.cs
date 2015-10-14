using FritzBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FritzBot.Functions
{
    public class Url
    {
        public string Protokoll { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Parameter { get; set; }

        public Url()
        {
            Protokoll = String.Empty;
            Host = String.Empty;
            Path = String.Empty;
            Parameter = new Dictionary<string, string>();
        }

        public Url(string url) : this()
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentException("url");
            }

            if (url.Contains("://"))
            {
                Protokoll = url.Substring(0, url.IndexOf(':'));
                url = url.Remove(0, Protokoll.Length + 3);
            }
            if (url.Contains("/"))
            {
                Host = url.Substring(0, url.IndexOf('/'));
                url = url.Remove(0, url.IndexOf('/'));

                if (url.Contains('?'))
                {
                    Path = url.Substring(0, url.IndexOf('?'));
                    url = url.Remove(0, url.IndexOf('?') + 1);

                    string[] ampersandSplit = url.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string split in ampersandSplit)
                    {
                        string[] equalsSplit = split.Split(new[] { "=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string key, value = String.Empty;

                        if (equalsSplit.Length == 0)
                        {
                            continue;
                        }

                        key = HttpUtility.UrlDecode(equalsSplit[0]);

                        if (equalsSplit.Length > 1)
                        {
                            value = HttpUtility.UrlDecode(equalsSplit[1]);
                        }
                        Parameter[key] = value;
                    }
                }
                else
                {
                    Path = url;
                }
            }
            else
            {
                Host = url;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(Protokoll))
            {
                sb.Append(Protokoll + "://");
            }
            sb.Append(Host);
            sb.Append(Path);
            if (Parameter.Count > 0)
            {
                sb.Append("?");
                sb.Append(Parameter.Select(x => $"{HttpUtility.UrlEncode(x.Key)}={HttpUtility.UrlEncode(x.Value)}").Join("&"));
            }
            return sb.ToString();
        }
    }
}