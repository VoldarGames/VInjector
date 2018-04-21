using System;
using System.Collections.Generic;
using System.Linq;
using VInjectorCore.Core;
using VInjectorCore.Exceptions;

namespace VInjectorCore
{
    public static class VInjector
    {
        internal static Dictionary<RegisteredType, RegisteredInstanceType> RegistrationDictionary
            = new Dictionary<RegisteredType, RegisteredInstanceType>();

        public static void Register<TInterface, TInstance>
            (LifeTime lifeTime = LifeTime.Global, TInstance instance = default(TInstance),
            int priority = 0, string registrationName = null)
            where TInstance : class, TInterface, new()
            where TInterface : class
        {
            if (RegistrationDictionary.Keys.Any(
                    type => type.InterfaceType == typeof(TInterface) 
                    && (type.RegistrationName.Equals(typeof(TInstance).Name) || type.RegistrationName.Equals(registrationName))))
            {
                throw new AlreadyRegisteredTypeVInjectorException(typeof(TInterface), typeof(TInstance));
            }
            if (lifeTime == LifeTime.NewInstance)
            {
                instance = null;
            }
            var registeredInstance = instance == null && lifeTime == LifeTime.Global ? Activator.CreateInstance<TInstance>() : instance;
            RegistrationDictionary.Add(
                new RegisteredType
                {
                RegistrationName = registrationName ?? typeof(TInstance).Name,
                Priority = priority,
                InterfaceType = typeof(TInterface),
                LifeTime = lifeTime
                },
                new RegisteredInstanceType
                {
                    Instance = registeredInstance,
                    InstanceType = typeof(TInstance)
                });
        }
        public static TInterface Resolve<TInterface>(string registrationName = null) where TInterface : class 
        {
            var priorizedRegisteredType = RegistrationDictionary.Keys.Where(type => type.InterfaceType == typeof(TInterface)
                                                                            && (registrationName == null || type.RegistrationName.Equals(registrationName)))
                                                                            .OrderBy(type => type.Priority)
                                                                            .FirstOrDefault();
            if (priorizedRegisteredType == null) throw new UnRegisteredTypeVInjectorException(typeof(TInterface), registrationName);

            var priorizedRegisteredInstance = RegistrationDictionary[priorizedRegisteredType];

            switch (priorizedRegisteredType.LifeTime)
            {
                case LifeTime.NewInstance:
                    return (TInterface)Activator.CreateInstance(priorizedRegisteredInstance.InstanceType);
                default: //LifeTime.Global
                    return (TInterface)priorizedRegisteredInstance.Instance;
            }
        }
    }
}