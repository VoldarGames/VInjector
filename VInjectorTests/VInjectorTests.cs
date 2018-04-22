﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VInjectorCore;
using VInjectorCore.Core;
using VInjectorCore.Exceptions;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class VInjectorTests
    {
        [TestCleanup]
        public void TearDown()
        {
            VInjector.RegistrationDictionary.Clear();    
        }

        [TestCategory("AutoRegister")]
        [TestMethod]
        public void AutoRegisterTypes_RegistrationOk()
        {
            VInjector.Initialize(this);
            Assert.AreEqual(1, VInjector.RegistrationDictionary.Count);
        }

        [TestCategory("Register_Failure")]
        [ExpectedException(typeof(AlreadyRegisteredTypeVInjectorException))]
        [TestMethod]
        public void RegisterSameType2TimesWithoutRegistrationName_ThrowAlreadyRegisteredException()
        {
            VInjector.Register<IDummy, Dummy>();
            VInjector.Register<IDummy, Dummy>();
        }

        [TestCategory("Register_Failure")]
        [ExpectedException(typeof(AlreadyRegisteredTypeVInjectorException))]
        [TestMethod]
        public void RegisterSameType2TimesWithSameRegistrationName_ThrowAlreadyRegisteredException()
        {
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, null, 0, "SameName");
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, null, 0, "SameName");
        }

        [TestCategory("Register_Success")]
        [TestMethod]
        public void RegisterType_RegisterOk()
        {
            VInjector.Register<IDummy, Dummy>();

            Assert.AreEqual(1, VInjector.RegistrationDictionary.Count);
            Assert.AreEqual(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.IsNotNull(VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [TestCategory("Register_Success")]
        [ExpectedException(typeof(AlreadyRegisteredTypeVInjectorException))]
        [TestMethod]
        public void RegisterSameType2TimesWithDifferentRegistrationName_RegisterOk()
        {
            var dummy1 = new Dummy() {Number = 1};
            var dummy2 = new Dummy() {Number = 2};
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy1);
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy2, 0, "DummyType2");

            Assert.AreSame(dummy1, VInjector.RegistrationDictionary.Values.Single(type => ((Dummy)type.Instance).Number == 1).Instance);
            Assert.AreSame(dummy2, VInjector.RegistrationDictionary.Values.Single(type => ((Dummy)type.Instance).Number == 2).Instance);
        }

        [TestCategory("Register_Success")]
        [TestMethod]
        public void RegisterInstance_RegisterOk()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummyInstance);

            Assert.AreEqual(1, VInjector.RegistrationDictionary.Count);
            Assert.AreEqual(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.AreSame(dummyInstance, VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [TestCategory("Register_Success")]
        [TestMethod]
        public void RegisterTypeWithoutParameters_RegisterParametersOk()
        {
            VInjector.Register<IDummy, Dummy>();

            Assert.AreEqual(typeof(Dummy).Name, VInjector.RegistrationDictionary.Keys.Single().RegistrationName);
            Assert.AreEqual(LifeTime.Global, VInjector.RegistrationDictionary.Keys.Single().LifeTime);
            Assert.AreEqual(0, VInjector.RegistrationDictionary.Keys.Single().Priority);
            Assert.AreEqual(typeof(IDummy), VInjector.RegistrationDictionary.Keys.Single().InterfaceType);
            Assert.AreEqual(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.IsNotNull(VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [TestCategory("Register_Success")]
        [TestMethod]
        public void RegisterInstanceWithParameters_RegisterParametersOk()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.NewInstance, dummyInstance, 1, "MyDummyInstance");

            Assert.AreEqual(1, VInjector.RegistrationDictionary.Count);
            Assert.AreEqual("MyDummyInstance", VInjector.RegistrationDictionary.Keys.Single().RegistrationName);
            Assert.AreEqual(LifeTime.NewInstance, VInjector.RegistrationDictionary.Keys.Single().LifeTime);
            Assert.AreEqual(1, VInjector.RegistrationDictionary.Keys.Single().Priority);
            Assert.AreEqual(typeof(IDummy), VInjector.RegistrationDictionary.Keys.Single().InterfaceType);
            Assert.AreEqual(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.AreNotSame(dummyInstance, VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [TestCategory("Resolve_Failure")]
        [ExpectedException(typeof(UnRegisteredTypeVInjectorException))]
        [TestMethod]
        public void ResolveUnregisteredInstance_ThrowUnregisteredException()
        {
            var instanceResult = VInjector.Resolve<IDummy>();
        }

        [TestCategory("Resolve_Failure")]
        [ExpectedException(typeof(UnRegisteredTypeVInjectorException))]
        [TestMethod]
        public void ResolveUnregisteredNamedInstance_ThrowUnregisteredException()
        {
            VInjector.Register<IComplexDummy, ComplexDummy>();
            var instanceResult = VInjector.Resolve<IDummy>("UnregisteredInstance");
        }

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void ResolveGlobalInstance_InstanceIsTheSame()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.AreSame(dummyInstance, instanceResult);
        }

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void ResolveUniqueNamedGlobalInstance_InstanceIsTheSame()
        {
            var dummy1 = new Dummy { Number = 1 };
            var dummy2 = new Dummy { Number = 2 };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy1, 0, "MyDummy_1");
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy2, 0, "MyDummy_2");

            var instanceResult = VInjector.Resolve<IDummy>("MyDummy_2");

            Assert.AreSame(dummy2, instanceResult);
        }

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void ResolveNewInstance_InstanceIsNotTheSame()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            //instance parameter must be ignored if NewInstance LifeTime is applied
            VInjector.Register<IDummy, Dummy>(LifeTime.NewInstance, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.AreNotSame(dummyInstance, instanceResult);
        }

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void ResolveGlobalInstanceWithDependencies_InstancesAreTheSame()
        {
            var dummyInstance = new Dummy
            {
                Number = 111
            };

            var complex = new ComplexDummy
            {
                Number = 222
            };

            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummyInstance);
            VInjector.Register<IComplexDummy, ComplexDummy>(LifeTime.Global, complex);

            var instanceResult = VInjector.Resolve<IComplexDummy>();

            Assert.AreSame(complex, instanceResult);
            Assert.AreSame(dummyInstance, instanceResult.Dummy);
            Assert.AreEqual(111, instanceResult.Dummy.Number);
            Assert.AreEqual(222, instanceResult.Number);
        }


    }
}
