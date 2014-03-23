using FritzBot.Core;
using System.Collections.Generic;

namespace FritzBot.DataModel
{
    public class Subscription : LinkedData<User>
    {
        public string Provider { get; set; }
        public string Plugin { get; set; }
        public List<string> Bedingungen { get; set; }

        public Subscription()
        {
            Bedingungen = new List<string>();
        }
    }
}