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
            (LifeTime lifeTime = LifeTime.Singleton, TInstance instance = default(TInstance),
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
            InjectProperties(resolvedInstance);
            return (TInterface)resolvedInstance;
        }

        private void InternalRegister(Type interfaceType, Type instanceType, LifeTime lifeTime, object instance, int priority, string registrationName)
        {
            ValidateRegister(interfaceType, instanceType, registrationName);
            var registeredInstance = GetInstanceToBeRegistered(instanceType, lifeTime, instance);

            RegistrationDictionary.Add(
                new RegisteredType
                {
                    RegistrationName = registrationName ?? instanceType.Name,
                    Priority = priority,
                    InterfaceType = interfaceType,
                    LifeTime = lifeTime
                },
                new RegisteredInstanceType
                {
                    Instance = registeredInstance,
                    InstanceType = instanceType
                });
        }

        private object GetInstanceToBeRegistered(Type instanceType, LifeTime lifeTime, object instance)
        {
            if (lifeTime == LifeTime.Transient)
            {
                instance = null;
            }
            return instance == null && lifeTime == LifeTime.Singleton ? Activator.CreateInstance(instanceType) : instance;
        }

        private void ValidateRegister(Type interfaceType, Type instanceType, string registrationName)
        {
            if (RegistrationDictionary.Keys.Any(
                            registeredType => registeredType.InterfaceType == interfaceType
                                    && ((registrationName == null && registeredType.RegistrationName.Equals(instanceType.Name))
                                    || registeredType.RegistrationName.Equals(registrationName)
                                    )))
            {
                throw new AlreadyRegisteredTypeVInjectorException(interfaceType, instanceType);
            }
        }

        private object InternalResolve(Type interfaceType, string registrationName = null)
        {
            var priorizedRegisteredType = GetPriorizedRegisteredType(interfaceType, registrationName);
            var priorizedRegisteredInstance = RegistrationDictionary[priorizedRegisteredType];

            switch (priorizedRegisteredType.LifeTime)
            {
                case LifeTime.Transient:
                    var ctorWithParameters = GetCtorWithVInjectCtorAttribute(priorizedRegisteredInstance);
                    if (ctorWithParameters == null)
                    {
                        return Activator.CreateInstance(priorizedRegisteredInstance.InstanceType);
                    }

                    var parameterInstances = ResolveCtorParameters(ctorWithParameters);
                    foreach (var parameterInstance in parameterInstances)
                    {
                        InjectProperties(parameterInstance);
                    }
                    return Activator.CreateInstance(priorizedRegisteredInstance.InstanceType, parameterInstances.ToArray());
                default: //LifeTime.Singleton
                    return priorizedRegisteredInstance.Instance;
            }
        }

        private List<object> ResolveCtorParameters(ConstructorInfo ctorWithParameters)
        {
            var autoInjectParameters = ctorWithParameters.GetCustomAttribute<VInjectCtor>().AutoInjectParameters;
            var parameters = ctorWithParameters.GetParameters();
            var parameterInstances = new List<object>();
            foreach (var parameter in parameters)
            {
                ResolveParameter(autoInjectParameters, parameterInstances, parameter);
            }

            return parameterInstances;
        }

        private void ResolveParameter(bool autoInjectParameters, List<object> parameterInstances, ParameterInfo parameter)
        {
            var vInjectParameter = parameter.GetCustomAttribute<VInjectParameter>();
            if (vInjectParameter != null)
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
            else if (autoInjectParameters && !parameter.ParameterType.GetTypeInfo().IsValueType)
            {
                parameterInstances.Add(InternalResolve(parameter.ParameterType));
            }
            else
            {
                parameterInstances.Add(parameter.ParameterType.GetTypeInfo().IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null);
            }
        }

        private static ConstructorInfo GetCtorWithVInjectCtorAttribute(RegisteredInstanceType priorizedRegisteredInstance)
        {
            return priorizedRegisteredInstance.InstanceType.GetConstructors().FirstOrDefault(ctor => ctor.GetCustomAttribute<VInjectCtor>() != null);
        }

        private static RegisteredType GetPriorizedRegisteredType(Type interfaceType, string registrationName)
        {
            var priorizedRegisteredType = RegistrationDictionary.Keys.Where(type => type.InterfaceType == interfaceType
                                                                                    && (registrationName == null || type.RegistrationName.Equals(registrationName)))
                .OrderBy(type => type.Priority)
                .FirstOrDefault();
            if (priorizedRegisteredType == null) throw new UnRegisteredTypeVInjectorException(interfaceType, registrationName);
            return priorizedRegisteredType;
        }

        private void InjectProperties(object instance)
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
                    InjectProperties(property.GetValue(instance));
                    LoopDetectionList.Remove(instanceType);
                }
            }
        }
    }
}