using System;

namespace VoldarGames.NetCore.VInjector.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class VInjectCtor : Attribute
    {
        internal bool AutoInjectParameters { get; set; } = true;
    }
}