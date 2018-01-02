using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace FritzBot.Functions
{
    public static class GoogleTranslator
    {
        public static string TranslateTextSimple(string Text, string DestinationLanguage)
        {
            return GetTranslation(Text, DestinationLanguage, "")?.FullTranslation;
        }

        public static Translation GetTranslation(string Text, string DestinationLanguage, string SourceLanguage)
        {
            string url = String.Format("http://translate.google.com/translate_a/t?client=p&text={0}&hl={1}&sl={2}&tl={1}&ie=UTF-8&oe=UTF-8&multires=1&otf=1&pc=1&trs=1&ssel=3&tsel=6&sc=1", Toolbox.UrlEncode(Text), DestinationLanguage, SourceLanguage); //"" Für Auto Erkennung der Ausgangssprache
            //string _l = String.Format("https://translate.google.com/translate_a/single?client=t&sl=en&tl=de&hl=de&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&ie=UTF-8&oe=UTF-8&otf=1&ssel=0&tsel=0&kc=11&tk=42047.455952&q=hello%20world")
            //string _rl = String.Format("https://translate.google.de/translate_a/single?client=t&sl=de&tl=en&hl=de&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&ie=UTF-8&oe=UTF-8&otf=1&ssel=0&tsel=0&kc=5&tk=488734.74801&q=hallo");
            WebClient dl = new WebClient();
            dl.Encoding = Encoding.UTF8;
            string response = dl.DownloadString(url);

            Translation translation = JsonConvert.DeserializeObject<Translation>(response);
            return translation;
        }

        public static string DetectLanguage(string Text)
        {
            return GetTranslation(Text, "", "")?.src;
        }
    }

    public class Translation
    {
        public Sentence[] sentences { get; set; }
        public Dic[] dict { get; set; }
        public Spell spell { get; set; }
        public string src { get; set; }
        public int server_time { get; set; }
        public string FullTranslation
        {
            get
            {
                return String.Concat(sentences.Select(x => x.trans));
            }
        }
    }

    public class Sentence
    {
        public string trans { get; set; }
        public string orig { get; set; }
        public string translit { get; set; }
        public string src_translit { get; set; }
    }

    public class Dic
    {
        public string pos { get; set; }
        public string[] terms { get; set; }
        public Entry[] entry { get; set; }
    }

    public class Entry
    {
        public string word { get; set; }
        public string[] reverse_translation { get; set; }
        public decimal score { get; set; }
    }

    public class Spell
    {
        public string spell_html_res { get; set; }
        public string spell_res { get; set; }
        public int[] correction_type { get; set; }
    }
}