using FritzBot.Core;

namespace FritzBot.DataModel
{
    public class WitzEntry : LinkedData<User>
    {
        public string Witz { get; set; }
    }
}
