using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.Core
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
            return list;
        }

        public static bool Contains<T>(this IEnumerable<T> list, T item, Func<T, object> bedingung)
        {
            return list.Contains(item, new KeyEqualityComparer<T>(bedingung));
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, object> bedingung)
        {
            return list.Distinct<T>(new KeyEqualityComparer<T>(bedingung));
        }

        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> list, IEnumerable<T> second, Func<T, object> bedingung)
        {
            return list.Intersect<T>(second, new KeyEqualityComparer<T>(bedingung));
        }

        public static List<T> Steal<T>(this List<T> list, Func<T, bool> bedingung)
        {
            List<T> extracted = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (bedingung(list[i]))
                {
                    extracted.Add(list[i]);
                    list.RemoveAt(i);
                    i--;
                }
            }
            return extracted;
        }
    }

    public static class XMLExtensions
    {
        public static T AddSingle<T>(this XContainer obj, T element)
        {
            obj.Add(element);
            return element;
        }

        public static XElement AddIfHasElements(this XElement target, XElement element)
        {
            if (element.HasElements)
            {
                target.Add(element);
            }
            return element;
        }

        public static XElement GetElementOrCreate(this XElement storage, string name)
        {
            XElement el = storage.Element(name);
            if (el == null)
            {
                el = new XElement(name);
                storage.Add(el);
            }
            return el;
        }

        public static string AttributeValueOrEmpty(this XElement element, string name)
        {
            XAttribute a = element.Attribute(name);
            if (a != null)
            {
                return element.Attribute(name).Value;
            }
            return String.Empty;
        }

        public static string ToStringWithDeclaration(this XDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            StringBuilder builder = new StringBuilder();
            using (TextWriter writer = new Utf8StringWriter(builder))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        public sealed class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder builder) : base(builder) { }
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }

    public static class HtmlDocumentExtensions
    {
        public static HtmlDocument LoadUrl(this HtmlDocument doc, string url)
        {
            string page = toolbox.GetWeb(url);
            if (String.IsNullOrEmpty(page))
            {
                throw new InvalidOperationException("Verbindungsfehler");
            }
            doc.LoadHtml(page);
            return doc;
        }

        public static HtmlNode GetHtmlNode(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        public static HtmlNode StripComments(this HtmlNode node)
        {
            foreach (HtmlNode item in node.Descendants().OfType<HtmlCommentNode>().ToList())
            {
                item.Remove();
            }
            return node;
        }

        public static HtmlDocument StripComments(this HtmlDocument doc)
        {
            doc.DocumentNode.StripComments();
            return doc;
        }

        public static IEnumerable<HtmlNode> Siblings(this HtmlNode node)
        {
            HtmlNode tmp = node.NextSibling;
            yield return tmp;
            while (tmp.NextSibling != null)
            {
                tmp = tmp.NextSibling;
                yield return tmp;
            }
        }
    }

    public static class UserExtensions
    {
        /// <summary>
        /// Gibt die Subscriptions des Users zurück
        /// </summary>
        public static IEnumerable<XElement> GetSubscriptions(this User user)
        {
            if (user.GetModulUserStorage("subscribe").Storage.Element("Subscriptions") != null)
            {
                return user.GetModulUserStorage("subscribe").Storage.Element("Subscriptions").Elements("Plugin");
            }
            return new List<XElement>();
        }
    }

    public class KeyEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> comparer;
        private readonly Func<T, object> keyExtractor;

        // Allows us to simply specify the key to compare with: y => y.CustomerID
        public KeyEqualityComparer(Func<T, object> keyExtractor) : this(keyExtractor, null) { }
        // Allows us to tell if two objects are equal: (x, y) => y.CustomerID == x.CustomerID
        public KeyEqualityComparer(Func<T, T, bool> comparer) : this(null, comparer) { }

        public KeyEqualityComparer(Func<T, object> keyExtractor, Func<T, T, bool> comparer)
        {
            this.keyExtractor = keyExtractor;
            this.comparer = comparer;
        }

        public bool Equals(T x, T y)
        {
            if (comparer != null)
                return comparer(x, y);
            else
            {
                var valX = keyExtractor(x);
                if (valX is IEnumerable<object>) // The special case where we pass a list of keys
                    return ((IEnumerable<object>)valX).SequenceEqual((IEnumerable<object>)keyExtractor(y));

                return valX.Equals(keyExtractor(y));
            }
        }

        public int GetHashCode(T obj)
        {
            if (keyExtractor == null)
                return obj.ToString().ToLower().GetHashCode();
            else
            {
                var val = keyExtractor(obj);
                if (val is IEnumerable<object>) // The special case where we pass a list of keys
                    return (int)((IEnumerable<object>)val).Aggregate((x, y) => x.GetHashCode() ^ y.GetHashCode());

                return val.GetHashCode();
            }
        }
    }
}