using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;
using VoldarGames.NetCore.VInjector.Core.Interfaces;
using VoldarGames.NetCore.VInjector.Exceptions;

[assembly:InternalsVisibleTo("VoldarGames.NetCore.VInjector.UnitTests")]

namespace VoldarGames.NetCore.VInjector
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
                    InternalRegister(autoRegisterAttribute.InterfaceType, type.AsType(),
                        autoRegisterAttribute.Lifetime, null, autoRegisterAttribute.Priority, autoRegisterAttribute.RegistrationName);
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
                        && (
                        (registrationName == null && type.RegistrationName.Equals(typeInstance.Name)) 
                        || type.RegistrationName.Equals(registrationName)
                        )
                        ))
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
                // if priorizedRegisteredInstance.InstanceType has ctor with parameters, then inject parameters; otherwise use activator to crete instance
                    var ctorWithParameters = priorizedRegisteredInstance.InstanceType.GetConstructors().FirstOrDefault(ctor => ctor.GetCustomAttribute<VInjectCtor>() != null);
                    if(ctorWithParameters == null)
                    {
                        return Activator.CreateInstance(priorizedRegisteredInstance.InstanceType);
                    }
                    var autoInjectParameters = ctorWithParameters.GetCustomAttribute<VInjectCtor>().AutoInjectParameters;
                    var parameters = ctorWithParameters.GetParameters();
                    var parameterInstances = new List<object>();
                    foreach (var parameter in parameters)
                    {
                        var vInjectParameter = parameter.GetCustomAttribute<VInjectParameter>();
                        if(vInjectParameter != null)
                        {
                            var parameterTypeDefaultValue = parameter.ParameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
                            if (!object.Equals(vInjectParameter.DefaultValue, parameterTypeDefaultValue))
                            {
                                parameterInstances.Add(vInjectParameter.DefaultValue);
                            }
                            else
                            {
                                parameterInstances.Add(InternalResolve(parameter.ParameterType, vInjectParameter.RegistrationName));
                            }
                        }
                        else if(autoInjectParameters && !parameter.ParameterType.GetTypeInfo().IsValueType)
                        {
                            parameterInstances.Add(InternalResolve(parameter.ParameterType));
                        }
                        else
                        {
                            parameterInstances.Add(parameter.ParameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null);
                        }
                    }
                   return Activator.CreateInstance(priorizedRegisteredInstance.InstanceType, parameterInstances.ToArray());
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