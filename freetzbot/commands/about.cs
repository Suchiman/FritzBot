using System;

namespace freetzbot.commands
{
    class about : command
    {
        private String[] name = { "about" };
        private String helptext = "Ich würde dir dann kurz etwas über mich erzählen.";
        private Boolean op_needed = false;
        private Boolean parameter_needed = false;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        private String zeilen = Convert.ToString(50 + 101 + 86 + 71 + 52 + 43 + 53 + 72 + 60 + 45 + 43 + 43 + 70 + 147 + 43 + 68 + 66 + 258 + 44 + 47 + 77 + 53 + 222 + 78 + 44 + 80 + 136 + 50 + 70 + 78 + 65 + 51 + 164 + 45 + 198 + 14 + 74 + 313 + 170 + 317);

        public void run(irc connection, String sender, String receiver, String message)
        {
            connection.sendmsg("Primäraufgabe: Daten über Fritzboxen sammeln, Sekundäraufgabe: Menschheit eliminieren. Kleiner Scherz am Rande Ha-Ha. Funktionsliste ist durch !hilfe zu erhalten. Programmiert in C# umfasst mein Quellcode derzeit " + zeilen + " Zeilen. Entwickler: Suchiman", receiver);
        }
    }
}