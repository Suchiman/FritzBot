using System;

namespace FritzBot
{
    interface ICommand
    {
        String[] Name { get; }
        String HelpText { get; }
        Boolean OpNeeded { get; }
        Boolean ParameterNeeded { get; }
        Boolean AcceptEveryParam { get; }
        void Run(Irc connection, String sender, String receiver, String message);
        void Destruct();
    }
}