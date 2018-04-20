using System;
using System.Collections.Generic;
using System.Linq;

namespace VInjectorCore
{
    public static class VInjector
    {
        internal static Dictionary<RegisteredType, RegisteredInstanceType> RegistrationDictionary
            = new Dictionary<RegisteredType, RegisteredInstanceType>();

        public static void Register<TInterface, TInstance>
            (LifeTime lifeTime = LifeTime.Global, TInstance instance = default(TInstance),
            int priority = 0, string registrationName = null)
            where TInstance : TInterface
        {
            if (RegistrationDictionary.Keys.Any(
                    type => type.InterfaceType == typeof(TInterface) 
                    && type.RegistrationName.Equals(typeof(TInstance).Name)))
            {
                throw new AlreadyRegisteredTypeVInjectorException(typeof(TInterface), typeof(TInstance));
            }
            var registeredInstance = instance == null && lifeTime == LifeTime.Global ? Activator.CreateInstance<TInstance>() : instance;
            RegistrationDictionary.Add(
                new RegisteredType
                {
                RegistrationName = registrationName ?? typeof(TInstance).Name,
                Priority = priority,
                InterfaceType = typeof(TInterface)
                },
                new RegisteredInstanceType
                {
                    Instance = registeredInstance,
                    InstanceType = typeof(TInstance)
                });
        }
    }

    public class AlreadyRegisteredTypeVInjectorException : Exception
    {
        public AlreadyRegisteredTypeVInjectorException(Type interfaceType, Type instanceType) 
            : base($"Interface {interfaceType.Name} is already registered with Type {instanceType.Name}") { }
    }

    public enum LifeTime
    {
        NewInstance,
        Global
    }

    internal class RegisteredType
    {
        public Type InterfaceType { get; set; }
        public string RegistrationName { get; set; }
        public int Priority { get; set; }
    }

    internal class RegisteredInstanceType
    {
        public Type InstanceType { get; set; }
        public object Instance { get; set; }
    }
}