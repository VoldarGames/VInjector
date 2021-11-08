using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VInjectorCore;
using VInjectorCore.Core;
using VInjectorCore.Core.Interfaces;
using VInjectorCore.Exceptions;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class VInjectorTests : IVAppContext
    {
        public VInjector VInjector;
        [TestInitialize]
        public void Initialize()
        {
            VInjector = new VInjector();
        }

        [TestCleanup]
        public void TearDown()
        {
            VInjector.RegistrationDictionary.Clear();    
        }

        [TestCategory("AutoRegister")]
        [TestMethod]
        public void AutoRegisterTypes_RegistrationOk()
        {
            VInjector.Initialize<VInjectorTests>();
            Assert.AreEqual(3, VInjector.RegistrationDictionary.Count);
        }

        [TestCategory("AutoRegister")]
        [TestMethod]
        public void AutoRegisterTypes_RetrieveOk()
        {
            VInjector.Initialize<VInjectorTests>();
            var instanceResult = VInjector.Resolve<IComplexDummy>();
            Assert.IsNotNull(instanceResult);
            Assert.IsNotNull(instanceResult.Dummy);
        }

        [TestCategory("AutoRegister")]
        [TestMethod]
        public void AutoRegisterTypesWithParams_RetrieveOk()
        {
            VInjector.Initialize<VInjectorTests>();
            var instanceResult = VInjector.Resolve<IMoreComplexDummy>();
            Assert.IsNotNull(instanceResult);
            Assert.IsNotNull(instanceResult.ComplexDummy);
            Assert.IsNotNull(instanceResult.ComplexDummy.Dummy);
            Assert.AreEqual("MoreComplex", VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).RegistrationName);
            Assert.AreEqual(LifeTime.NewInstance, VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).LifeTime);
            Assert.AreEqual(1, VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).Priority);
            Assert.AreEqual(typeof(IMoreComplexDummy), VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).InterfaceType);
            Assert.AreEqual(typeof(MoreComplexDummy), VInjector.RegistrationDictionary.Values.Single(type => type.InstanceType == typeof(MoreComplexDummy)).InstanceType);
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
            var instanceResult = VInjector.Resolve<IComplexDummy>("UnregisteredInstance");
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

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void ResolveGlobalInstanceWithNestedDependencies_InstancesAreTheSame()
        {
            var moreComplexDummy = new MoreComplexDummy
            {
                Name = "1234"
            };

            var complex = new ComplexDummy()
            {
                Number = 111
            };

            var dummy = new Dummy()
            {
                Number = 222
            };

            VInjector.Register<IMoreComplexDummy, MoreComplexDummy>(LifeTime.Global, moreComplexDummy);
            VInjector.Register<IComplexDummy, ComplexDummy>(LifeTime.Global, complex);
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy);

            var instanceResult = VInjector.Resolve<IMoreComplexDummy>();

            Assert.AreSame(moreComplexDummy, instanceResult);
            Assert.AreSame(complex, instanceResult.ComplexDummy);
            Assert.AreSame(dummy, instanceResult.ComplexDummy.Dummy);
            Assert.AreEqual("1234",instanceResult.Name);
            Assert.AreEqual(111,instanceResult.ComplexDummy.Number);
            Assert.AreEqual(222,instanceResult.ComplexDummy.Dummy.Number);
        }

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void ResolveGlobalInstanceWithCyclicDependencies_InstancesAreTheSame()
        {
            var cyclic1 = new CyclicDependencyDummyPart1();
            var cyclic2 = new CyclicDependencyDummyPart2();
            VInjector.Register<ICyclicDependencyDummyPart1, CyclicDependencyDummyPart1>(LifeTime.Global, cyclic1);
            VInjector.Register<ICyclicDependencyDummyPart2, CyclicDependencyDummyPart2>(LifeTime.Global, cyclic2);

            var instanceResult = VInjector.Resolve<ICyclicDependencyDummyPart1>();
            Assert.AreSame(cyclic1, instanceResult);
            Assert.AreSame(cyclic2, instanceResult.Part2);
        }

        [TestCategory("Resolve_Success")]
        [TestMethod]
        public void MockVInjector_Success()
        {
            var mockVInjector = new Mock<IVInjector>() { CallBase = true };
            var dummy = new Dummy { Number = 12345678 };

            mockVInjector.Setup(injector => injector.Resolve<IDummy>(It.IsAny<string>())).Returns(dummy);

            mockVInjector.Object.Initialize<VInjectorTests>();

            var result = mockVInjector.Object.Resolve<IDummy>();

            Assert.AreSame(dummy, result);

        }
    }
}
