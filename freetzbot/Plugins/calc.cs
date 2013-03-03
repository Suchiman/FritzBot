﻿using FritzBot.DataModel;
using System;
using System.Net;
using System.Web.Script.Serialization;

namespace FritzBot.Plugins
{
    [Module.Name("calc")]
    [Module.Help("Ich kann sogar Rechnen :-) !calc 42*13+1 !calc 42*(42-(24+24)+1*3)/2")]
    [Module.ParameterRequired]
    class calc : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            WebClient dl = new WebClient();
            string url = String.Format("http://www.google.com/ig/calculator?q={0}", toolbox.UrlEncode(theMessage.CommandLine));
            string result = dl.DownloadString(url);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            CalculationResult cr = jss.Deserialize<CalculationResult>(result);
            if (String.IsNullOrEmpty(cr.error))
            {
                theMessage.Answer(String.Format("{0} ergibt {1}", cr.lhs, cr.rhs));
            }
            else
            {
                theMessage.Answer("Die Eingabe ist ungültig oder konnte nicht interpretiert werden");
            }
        }
    }

    class CalculationResult
    {
        public string lhs { get; set; }
        public string rhs { get; set; }
        public string error { get; set; }
        public bool icc { get; set; }
    }
}