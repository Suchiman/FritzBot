using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FritzBot.DataModel.LegacyModels
{
    public class SimpleStorage : LinkedData<object>
    {
        public string ID { get; set; }
        public Dictionary<string, object> _values = new Dictionary<string, object>();

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
            Contract.Requires(key != null);

            if (!_values.ContainsKey(key))
            {
                return @default;
            }
            return (T)_values[key];
        }

        public void Store(string key, object value)
        {
            Contract.Requires(key != null);

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