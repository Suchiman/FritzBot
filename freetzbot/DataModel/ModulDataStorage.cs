using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FritzBot.DataModel
{
    public class ModulDataStorage
    {
        private XElement storage;

        public XElement Storage { get { return storage; } }

        public ModulDataStorage(XElement daten)
        {
            if (daten != null)
            {
                storage = daten;
            }
            else
            {
                throw new ArgumentNullException("daten", "Es muss ein gültiges XElement übergeben werden");
            }
        }

        public void SetVariable(string name, object value)
        {
            if (storage.Element(name) != null)
            {
                storage.Element(name).SetValue(value);
            }
            else
            {
                storage.Add(new XElement(name, value));
            }
        }

        public string GetVariable(string name)
        {
            XElement xe = storage.Element(name);
            if (xe != null)
            {
                return xe.Value;
            }
            return null;
        }

        public string GetVariable(string name, string @default)
        {
            return GetVariable(name) ?? @default;
        }

        public XElement GetElement(string name, bool CreateIfNotExist)
        {
            XElement el = storage.Element(name);
            if (el == null && CreateIfNotExist)
            {
                el = new XElement(name);
                storage.Add(el);
            }
            return el;
        }

        public XElement GetNewElement(string name)
        {
            XElement el = new XElement(name);
            storage.Add(el);
            return el;
        }
    }
}