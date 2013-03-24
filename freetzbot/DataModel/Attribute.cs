using System;
using System.Linq;

namespace FritzBot.Module
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class NameAttribute : Attribute
    {
        public string[] Names;

        public NameAttribute(params string[] names)
        {
            Names = names;
        }

        public override bool Match(object obj)
        {
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

        public bool IsNamed(string name)
        {
            return Names.Any(x => x.ToLower() == name.ToLower());
        }

        public static bool IsNamed(object obj, string name)
        {
            NameAttribute na = toolbox.GetAttribute<NameAttribute>(obj);
            return na != null && na.IsNamed(name);
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class HelpAttribute : Attribute
    {
        public string Help;

        public HelpAttribute(string help)
        {
            Help = help;
        }
    }

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

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute() { }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class HiddenAttribute : Attribute
    {
        public HiddenAttribute() { }

        public static bool CheckHidden(Object obj)
        {
            return toolbox.GetAttribute<HiddenAttribute>(obj) != null;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class SubscribeableAttribute : Attribute
    {
        public SubscribeableAttribute() { }
    }
}