using System;
using System.Reflection;

namespace FritzBot.Plugins
{
    /// <summary>
    /// Definiert die Namen / Shortcuts über das das Plugin angesprochen wird
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class NameAttribute : Attribute
    {
        public string[] Names;

        public NameAttribute(params string[] names)
        {
            Names = names;
        }

        public override bool Match(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            NameAttribute obj1 = (NameAttribute)obj;
            if (obj1.Names.Length != Names.Length)
            {
                return false;
            }
            for (int i = 0; i < Names.Length; i++)
            {
                if (obj1.Names[i] != Names[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Gibt einen Hilfetext für dieses Plugin an
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class HelpAttribute : Attribute
    {
        public string Help;

        public HelpAttribute(string help)
        {
            Help = help;
        }
    }

    /// <summary>
    /// Wenn gesetzt, bestimmt sein Wert ob ein gültiger Aufruf Attribute haben muss oder nicht.
    /// Andernfalls wird nicht geprüft ob Werte übergeben werden müssen oder nicht
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ParameterRequiredAttribute : Attribute
    {
        public bool Required;

        public ParameterRequiredAttribute()
        {
            Required = true;
        }

        public ParameterRequiredAttribute(bool parameterRequired)
        {
            Required = parameterRequired;
        }
    }

    /// <summary>
    /// Gibt an, dass der Benutzer über Admin berechtigungen verfügen muss
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute
    {
    }

    /// <summary>
    /// Gibt an, dass dieses Plugin vor der Hilfe und anderen listenden Funktionen versteckt werden soll
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class HiddenAttribute : Attribute
    {
        public static bool CheckHidden(object obj)
        {
            return obj.GetType().GetCustomAttribute<HiddenAttribute>() != null;
        }
    }

    /// <summary>
    /// Gibt an, dass dieses Plugin benachrichtigungen für Subscriber unterstützt
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class SubscribeableAttribute : Attribute
    {
    }

    /// <summary>
    /// Definiert den Scope einer Instanz dieses Plugins. Wenn nicht oder nicht anders gesetzt, ist der Standardwert Global
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ScopeAttribute : Attribute
    {
        public Scope Scope { get; set; }

        public ScopeAttribute(Scope scope)
        {
            Scope = scope;
        }
    }
}

namespace FritzBot
{
    public enum Scope
    {
        /// <summary>
        /// Eine Instanz gilt für alle User in jedem Channel
        /// </summary>
        Global,
        /// <summary>
        /// Eine Instanz gilt nur für einen User
        /// </summary>
        User,
        /// <summary>
        /// Eine Instanz gilt nur pro Channel
        /// </summary>
        Channel,
        /// <summary>
        /// Eine Instanz gilt für einen User pro Channel
        /// </summary>
        UserChannel
    }
}