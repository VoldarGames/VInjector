using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VInjectorCore.Attributes;
using VInjectorCore.Core;
using VInjectorCore.Exceptions;

namespace VInjectorCore
{
    public static class VInjector
    {
        private static List<Type> LoopDetectionList = new List<Type>();
        internal static Dictionary<RegisteredType, RegisteredInstanceType> RegistrationDictionary
            = new Dictionary<RegisteredType, RegisteredInstanceType>();

        public static void Initialize<T>(T assemblyType)
        {
            var assemblyName = typeof(T).AssemblyQualifiedName;
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            foreach (var type in assembly.DefinedTypes)
            {
                var autoRegisterAttribute = type.GetCustomAttribute<VAutoRegister>();
                if (autoRegisterAttribute != null)
                {
                    InternalRegister(autoRegisterAttribute.InterfaceType, type.DeclaringType,
                        autoRegisterAttribute.Lifetime, null, autoRegisterAttribute.Priority, autoRegisterAttribute.RegistrationName);
                }
            }
        }

        private static void InternalRegister(Type typeInterface, Type typeInstance, LifeTime lifeTime, object instance, int priority, string registrationName)
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

        static object InternalResolve(Type interfaceType, string registrationName = null)
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

        public static void Register<TInterface, TInstance>
            (LifeTime lifeTime = LifeTime.Global, TInstance instance = default(TInstance),
            int priority = 0, string registrationName = null)
            where TInstance : class, TInterface, new()
            where TInterface : class
        {
            InternalRegister(typeof(TInterface), typeof(TInstance), lifeTime, instance, priority, registrationName);
        }

        public static TInterface Resolve<TInterface>(string registrationName = null) where TInterface : class
        {
            LoopDetectionList.Clear();
            var resolvedInstance = (TInterface)InternalResolve(typeof(TInterface), registrationName);
            Inject(resolvedInstance);
            return resolvedInstance;
        }

        private static void Inject(object instance)
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