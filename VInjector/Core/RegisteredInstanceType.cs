using System;

namespace VInjectorCore.Core
{
    internal class RegisteredInstanceType
    {
        public Type InstanceType { get; set; }
        public object Instance { get; set; }
    }
}