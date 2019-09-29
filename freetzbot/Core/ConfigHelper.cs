using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace FritzBot.Core
{
    public static class ConfigHelper
    {
        private static readonly JObject conf;

        static ConfigHelper()
        {
            string configPath = GetPath();
            if (File.Exists(configPath))
            {
                using (var text = File.OpenText(configPath))
                {
                    conf = (JObject)JToken.ReadFrom(new JsonTextReader(text));
                }
            }
            else
            {
                conf = new JObject();
            }
        }

        private static string GetPath()
        {
            return Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "config.json");
        }

        private static void SaveChanges()
        {
            string configPath = GetPath();
            using (var text = File.CreateText(configPath))
            {
                conf.WriteTo(new JsonTextWriter(text));
            }
        }

        public static bool KeyExists(string key)
        {
            return conf[key] != null;
        }

        public static void Remove(string key)
        {
            if (KeyExists(key))
            {
                conf.Remove(key);
                SaveChanges();
            }
        }

        public static void SetValue(string key, string value)
        {
            conf[key] = value;
            SaveChanges();
        }

        public static string GetString(string key)
        {
            return conf[key].Value<string>();
        }

        public static string GetString(string key, string @default)
        {
            if (!KeyExists(key))
            {
                SetValue(key, @default);
            }
            return GetString(key);
        }

        public static int GetInt(string key)
        {
            string value = GetString(key);
            return Convert.ToInt32(value);
        }

        public static int GetInt(string key, int @default)
        {
            if (!KeyExists(key))
            {
                SetValue(key, @default.ToString());
            }
            return GetInt(key);
        }

        public static bool GetBoolean(string key)
        {
            string value = GetString(key);
            return Convert.ToBoolean(value);
        }

        public static bool GetBoolean(string key, bool @default)
        {
            if (!KeyExists(key))
            {
                SetValue(key, @default.ToString());
            }
            return GetBoolean(key);
        }
    }
}