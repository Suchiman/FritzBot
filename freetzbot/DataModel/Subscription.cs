using FritzBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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