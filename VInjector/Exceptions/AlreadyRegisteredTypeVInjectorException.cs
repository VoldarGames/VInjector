using System;

namespace VInjectorCore.Exceptions
{
    public class AlreadyRegisteredTypeVInjectorException : Exception
    {
        public AlreadyRegisteredTypeVInjectorException(Type interfaceType, Type instanceType) 
            : base($"Interface {interfaceType.Name} is already registered with Type {instanceType.Name}") { }
    }
}