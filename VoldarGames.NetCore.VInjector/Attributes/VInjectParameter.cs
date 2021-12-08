using System;

namespace VoldarGames.NetCore.VInjector.Attributes
{    
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class VInjectParameter : Attribute
    {
        internal string RegistrationName { get; private set; }

        internal object DefaultValue { get; private set; }

        public VInjectParameter(string registrationName = null, object defaultValue = null)
        {
            RegistrationName = registrationName;
            DefaultValue = defaultValue;
        }
    }
}