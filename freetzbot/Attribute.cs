using System;
using System.Collections.Generic;
using System.Text;

namespace FritzBot.Module
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class NameAttribute : Attribute
    {
        public String[] Names;

        public NameAttribute(params String[] names)
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

        public Boolean IsNamed(String name)
        {
            foreach (String derName in Names)
            {
                if (derName.ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class HelpAttribute : Attribute
    {
        public String Help;

        public HelpAttribute(String help)
        {
            Help = help;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ParameterRequiredAttribute : Attribute
    {
        public Boolean Required;

        public ParameterRequiredAttribute()
        {
            Required = true;
        }

        public ParameterRequiredAttribute(Boolean parameterRequired)
        {
            Required = parameterRequired;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute() { }
    }
}
