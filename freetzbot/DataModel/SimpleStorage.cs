using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzBot.DataModel
{
    public class SimpleStorage : LinkedData<object>
    {
        public string ID { get; set; }
        private Dictionary<string, object> _values = new Dictionary<string, object>();

        public SimpleStorage()
        {
            _values = new Dictionary<string, object>();
        }

        public T Get<T>(string key)
        {
            if (!_values.ContainsKey(key))
            {
                throw new Exception("Der angeforderte Wert ist nicht vorhanden");
            }
            return (T)_values[key];
        }

        public T Get<T>(string key, T @default)
        {
            if (!_values.ContainsKey(key))
            {
                return @default;
            }
            return (T)_values[key];
        }

        public void Store(string key, object value)
        {
            if (_values.ContainsKey(key))
            {
                _values[key] = value;
            }
            else
            {
                _values.Add(key, value);
            }
        }
    }
}