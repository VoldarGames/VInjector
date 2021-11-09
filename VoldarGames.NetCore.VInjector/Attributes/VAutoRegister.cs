using System;
using VoldarGames.NetCore.VInjector.Core;

namespace VoldarGames.NetCore.VInjector.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VAutoRegister : Attribute
    {
        internal Type InterfaceType { get; set; }
        internal LifeTime Lifetime { get; set; }
        internal int Priority { get; set; }
        internal string RegistrationName { get; set; }
        
        public VAutoRegister(Type interfaceType, LifeTime lifetime = LifeTime.Global, int priority = 0, string registrationName = null)
        {
            InterfaceType = interfaceType;
            Lifetime = lifetime;
            Priority = priority;
            RegistrationName = registrationName;
        }
    }
}
