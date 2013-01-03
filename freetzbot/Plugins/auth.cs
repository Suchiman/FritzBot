using FritzBot.DataModel;

namespace FritzBot.Plugins
{
    [Module.Name("auth")]
    [Module.Help("Authentifiziert dich wenn du ein Passwort festgelegt hast. z.b. !auth passwort")]
    [Module.ParameterRequired]
    class auth : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            if (theMessage.TheUser.Authenticated)
            {
                theMessage.Answer("Du bist bereits authentifiziert");
                return;
            }
            if (!theMessage.IsPrivate)
            {
                theMessage.Answer("Ohje das solltest du besser im Query tuen");
                return;
            }
            if (theMessage.TheUser.CheckPassword(theMessage.CommandLine))
            {
                theMessage.TheUser.Authenticated = true;
                theMessage.SendPrivateMessage("Du bist jetzt authentifiziert");
            }
            else
            {
                theMessage.SendPrivateMessage("Das Passwort war falsch");
            }
            theMessage.Hidden = true;
        }
    }
}
