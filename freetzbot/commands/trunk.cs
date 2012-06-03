using System;

namespace FritzBot.commands
{
    class trunk : ICommand
    {
        public String[] Name { get { return new String[] { "trunk" }; } }
        public String HelpText { get { return "Dies zeigt den aktuellsten Changeset an."; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return false; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(ircMessage theMessage)
        {
            String webseite = toolbox.GetWeb("http://freetz.org/changeset");
            if (!String.IsNullOrEmpty(webseite))
            {
                String changeset = "Der aktuellste Changeset ist " + webseite.Split(new String[] { "<h1>" }, 2, StringSplitOptions.None)[1].Split(new String[] { "</h1>" }, 2, StringSplitOptions.None)[0].Split(new String[] { "Changeset " }, 2, StringSplitOptions.None)[1];
                changeset += " und wurde am" + webseite.Split(new String[] { "<dd class=\"time\">" }, 2, StringSplitOptions.None)[1].Split(new String[] { "\n" }, 3, StringSplitOptions.None)[1].Split(new String[] { "   " }, 5, StringSplitOptions.None)[4] + " in den Trunk eingecheckt. Siehe: http://freetz.org/changeset";
                theMessage.Answer(changeset);
            }
            else
            {
                theMessage.Answer("Leider war es mir nicht möglich auf die Freetz Webseite zuzugreifen");
            }
        }
    }
}