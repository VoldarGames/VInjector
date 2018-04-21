using System;

namespace VInjectorCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VInject : Attribute
    {
        internal string RegistrationName { get; set; }

        public VInject(string registrationName = null)
        {
            RegistrationName = registrationName;
        }
    }
}