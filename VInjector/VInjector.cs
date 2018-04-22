using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VInjectorCore.Attributes;
using VInjectorCore.Core;
using VInjectorCore.Core.Interfaces;
using VInjectorCore.Exceptions;

namespace VInjectorCore
{
    public class VInjector : IVInjector
    {
        private static readonly List<Type> LoopDetectionList = new List<Type>();
        internal static Dictionary<RegisteredType, RegisteredInstanceType> RegistrationDictionary
            = new Dictionary<RegisteredType, RegisteredInstanceType>();

        public void Initialize<TVAppContext>() where TVAppContext : IVAppContext
        {
            foreach (var type in typeof(TVAppContext).GetTypeInfo().Assembly.DefinedTypes)
            {
                var autoRegisterAttribute = type.GetCustomAttribute<VAutoRegister>();
                if (autoRegisterAttribute != null)
                {
                    InternalRegister(autoRegisterAttribute.InterfaceType, type.DeclaringType,
                        autoRegisterAttribute.Lifetime, type, autoRegisterAttribute.Priority, autoRegisterAttribute.RegistrationName);
                }
            }
        }

        public void Register<TInterface, TInstance>
            (LifeTime lifeTime = LifeTime.Global, TInstance instance = default(TInstance),
            int priority = 0, string registrationName = null)
            where TInstance : class, TInterface, new()
            where TInterface : class
        {
            InternalRegister(typeof(TInterface), typeof(TInstance), lifeTime, instance, priority, registrationName);
        }

        public TInterface Resolve<TInterface>(string registrationName = null) where TInterface : class
        {
            LoopDetectionList.Clear();
            var resolvedInstance = InternalResolve(typeof(TInterface), registrationName);
            Inject(resolvedInstance);
            return (TInterface)resolvedInstance;
        }

        void InternalRegister(Type typeInterface, Type typeInstance, LifeTime lifeTime, object instance, int priority, string registrationName)
        {
            if (RegistrationDictionary.Keys.Any(
                type => type.InterfaceType == typeInterface
                        && (type.RegistrationName.Equals(typeInstance.Name) || type.RegistrationName.Equals(registrationName))))
            {
                throw new AlreadyRegisteredTypeVInjectorException(typeInterface, typeInstance);
            }
            if (lifeTime == LifeTime.NewInstance)
            {
                instance = null;
            }
            var registeredInstance = instance == null && lifeTime == LifeTime.Global ? Activator.CreateInstance(typeInstance) : instance;
            RegistrationDictionary.Add(
                new RegisteredType
                {
                    RegistrationName = registrationName ?? typeInstance.Name,
                    Priority = priority,
                    InterfaceType = typeInterface,
                    LifeTime = lifeTime
                },
                new RegisteredInstanceType
                {
                    Instance = registeredInstance,
                    InstanceType = typeInstance
                });
        }

        object InternalResolve(Type interfaceType, string registrationName = null)
        {
            var priorizedRegisteredType = RegistrationDictionary.Keys.Where(type => type.InterfaceType == interfaceType
                                                                                    && (registrationName == null || type.RegistrationName.Equals(registrationName)))
                .OrderBy(type => type.Priority)
                .FirstOrDefault();
            if (priorizedRegisteredType == null) throw new UnRegisteredTypeVInjectorException(interfaceType, registrationName);

            var priorizedRegisteredInstance = RegistrationDictionary[priorizedRegisteredType];

            switch (priorizedRegisteredType.LifeTime)
            {
                case LifeTime.NewInstance:
                    return Activator.CreateInstance(priorizedRegisteredInstance.InstanceType);
                default: //LifeTime.Global
                    return priorizedRegisteredInstance.Instance;
            }
        }

        void Inject(object instance)
        {
            var instanceType = instance.GetType();
            if (LoopDetectionList.Count(t => t == instanceType) > 1) return;
            foreach (var property in instance.GetType().GetRuntimeProperties())
            {
                var vInjectAttribute = property.GetCustomAttribute<VInject>();
                if (vInjectAttribute != null)
                {
                    property.SetValue(instance, InternalResolve(property.PropertyType, vInjectAttribute.RegistrationName));
                    LoopDetectionList.Add(instanceType);
                    Inject(property.GetValue(instance));
                    LoopDetectionList.Remove(instanceType);
                }
            }
        }
    }
}