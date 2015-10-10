using AngleSharp.Dom.Html;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.Core
{
    public static class Linq
    {
        public static IEnumerable<T> TryLogEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                try
                {
                    action(item);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Fehler beim batchen");
                }
            }
            return list;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            Contract.Requires(list != null);

            foreach (T item in list)
            {
                action(item);
            }
            return list;
        }

        public static bool Contains<T>(this IEnumerable<T> list, T item, Func<T, object> selector)
        {
            return list.Contains(item, new KeyEqualityComparer<T>(selector));
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, object> selector)
        {
            Contract.Requires(list != null);

            return list.Distinct<T>(new KeyEqualityComparer<T>(selector));
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

        public static IEnumerable<T> JoinMany<T>(params IEnumerable<T>[] items)
        {
            foreach (IEnumerable<T> item in items)
            {
                foreach (T subitem in item)
                {
                    yield return subitem;
                }
            }
        }

        public static string Join(this IEnumerable<string> source, string seperator)
        {
            return String.Join(seperator, source);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (T thisLevel in source)
            {
                foreach (T deepOne in Flatten(selector(thisLevel), selector))
                {
                    yield return deepOne;
                }
                yield return thisLevel;
            }
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source) where T : class
        {
            foreach (T element in source)
            {
                if (element != null)
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TR> FullOuterGroupJoin<TA, TB, TK, TR>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TK> selectKeyA,
            Func<TB, TK> selectKeyB,
            Func<IEnumerable<TA>, IEnumerable<TB>, TK, TR> projection,
            IEqualityComparer<TK> cmp = null)
        {
            cmp = cmp ?? EqualityComparer<TK>.Default;
            ILookup<TK, TA> alookup = a.ToLookup(selectKeyA, cmp);
            ILookup<TK, TB> blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            return from key in keys
                   let xa = alookup[key]
                   let xb = blookup[key]
                   select projection(xa, xb, key);
        }

        public static IEnumerable<TR> FullOuterJoin<TA, TB, TK, TR>(
            this IEnumerable<TA> a,
            IEnumerable<TB> b,
            Func<TA, TK> selectKeyA,
            Func<TB, TK> selectKeyB,
            Func<TA, TB, TK, TR> projection,
            TA defaultA = default(TA),
            TB defaultB = default(TB),
            IEqualityComparer<TK> cmp = null)
        {
            cmp = cmp ?? EqualityComparer<TK>.Default;
            ILookup<TK, TA> alookup = a.ToLookup(selectKeyA, cmp);
            ILookup<TK, TB> blookup = b.ToLookup(selectKeyB, cmp);

            var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
            keys.UnionWith(blookup.Select(p => p.Key));

            return from key in keys
                   from xa in alookup[key].DefaultIfEmpty(defaultA)
                   from xb in blookup[key].DefaultIfEmpty(defaultB)
                   select projection(xa, xb, key);
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
            Contract.Requires(doc != null);
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
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

    public static class HtmlExtensions
    {
        public static string HrefOrNull(this IHtmlAnchorElement a)
        {
            return a.HasAttribute("href") ? a.Href : null;
        }
    }

    public static class PluginExtensions
    {
        public static T As<T>(this PluginInfo info) where T : class
        {
            if (info == null)
            {
                return null;
            }
            return info.Plugin as T;
        }
    }

    public static class OtherExtensions
    {
        public static bool In<T>(this T source, params T[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length > 0);

            return values.Contains(source);
        }

        public static string SanitizeString(this string source)
        {
            return String.IsNullOrWhiteSpace(source) ? null : source.Trim();
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
            object valX = keyExtractor(x);
            if (valX is IEnumerable<object>) // The special case where we pass a list of keys
                return ((IEnumerable<object>)valX).SequenceEqual((IEnumerable<object>)keyExtractor(y));

            return valX.Equals(keyExtractor(y));
        }

        public int GetHashCode(T obj)
        {
            if (keyExtractor == null)
                return obj.ToString().ToLower().GetHashCode();
            object val = keyExtractor(obj);
            if (val is IEnumerable<object>) // The special case where we pass a list of keys
                return (int)((IEnumerable<object>)val).Aggregate((x, y) => x.GetHashCode() ^ y.GetHashCode());

            return val.GetHashCode();
        }
    }
}