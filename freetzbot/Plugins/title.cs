using FritzBot.DataModel;
using System;

namespace FritzBot.Plugins
{
    [Module.Name("title")]
    [Module.Help("Ruft den Titel der Webseite ab")]
    [Module.ParameterRequired]
    class title : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                String webpage = toolbox.GetWeb(theMessage.CommandArgs[0]);
                String title = webpage.Split(new String[] { "<title>" }, 8, StringSplitOptions.None)[1].Split(new String[] { "</title>" }, 2, StringSplitOptions.None)[0];
                while (title.IndexOf('\n') != -1)
                {
                    title = title.Remove(title.IndexOf('\n'), 1);
                }
                while (title.Contains("  "))
                {
                    title = title.Replace("  ", " ");
                }
                if(title.ToCharArray()[0] == ' ')
                {
                    title = title.Remove(0, 1);
                }
                if (title.ToCharArray()[title.ToCharArray().Length - 1] == ' ')
                {
                    title = title.Remove(title.ToCharArray().Length - 1, 1);
                }
                theMessage.Answer(title);
            }
            catch
            {
                theMessage.Answer("Entweder hat die Webseite keine Überschrift oder die URL ist nicht gültig");
            }
        }
    }
}
