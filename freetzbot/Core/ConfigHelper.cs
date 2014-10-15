using System;
using System.Configuration;
using System.Linq;

namespace FritzBot.Core
{
    public static class ConfigHelper
    {
        private static Configuration conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static bool KeyExists(string key)
        {
            return conf.AppSettings.Settings.AllKeys.Contains(key);
        }

        public static void Remove(string key)
        {
            if (KeyExists(key))
            {
                conf.AppSettings.Settings.Remove(key);
                conf.Save();
            }
        }

        public static void SetValue(string key, string value)
        {
            if (KeyExists(key))
            {
                conf.AppSettings.Settings.Remove(key);
            }
            conf.AppSettings.Settings.Add(key, value);
            conf.Save();
        }

        public static string GetString(string key)
        {
            KeyValueConfigurationElement entry = conf.AppSettings.Settings[key];
            if (entry == null)
            {
                throw new ConfigurationErrorsException();
            }
            return entry.Value;
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