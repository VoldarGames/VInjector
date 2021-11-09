using System;

namespace VoldarGames.NetCore.VInjector.Core
{
    internal class RegisteredInstanceType
    {
        public Type InstanceType { get; set; }
        public object Instance { get; set; }
    }
}