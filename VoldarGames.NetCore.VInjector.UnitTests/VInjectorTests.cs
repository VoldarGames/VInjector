using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using VoldarGames.NetCore.VInjector;
using VoldarGames.NetCore.VInjector.Core;
using VoldarGames.NetCore.VInjector.Core.Interfaces;
using VoldarGames.NetCore.VInjector.Exceptions;
using Xunit;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    public class VInjectorTests : IVAppContext
    {
        public VInjector VInjector;


        public VInjectorTests()
        {
            VInjector = new VInjector();
        }

        [Trait("Category", "AutoRegister")]
        [Fact]
        public void AutoRegisterTypes_RegistrationOk()
        {
            VInjector.Initialize<VInjectorTests>();
            Assert.Equal(3, VInjector.RegistrationDictionary.Count);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "AutoRegister")]
        [Fact]
        public void AutoRegisterTypes_RetrieveOk()
        {
            VInjector.Initialize<VInjectorTests>();
            var instanceResult = VInjector.Resolve<IComplexDummy>();
            Assert.NotNull(instanceResult);
            Assert.NotNull(instanceResult.Dummy);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "AutoRegister")]
        [Fact]
        public void AutoRegisterTypesWithParams_RetrieveOk()
        {
            VInjector.Initialize<VInjectorTests>();
            var instanceResult = VInjector.Resolve<IMoreComplexDummy>();
            Assert.NotNull(instanceResult);
            Assert.NotNull(instanceResult.ComplexDummy);
            Assert.NotNull(instanceResult.ComplexDummy.Dummy);
            Assert.Equal("MoreComplex", VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).RegistrationName);
            Assert.Equal(LifeTime.NewInstance, VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).LifeTime);
            Assert.Equal(1, VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).Priority);
            Assert.Equal(typeof(IMoreComplexDummy), VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).InterfaceType);
            Assert.Equal(typeof(MoreComplexDummy), VInjector.RegistrationDictionary.Values.Single(type => type.InstanceType == typeof(MoreComplexDummy)).InstanceType);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Failure")]
        [Fact]
        public void RegisterSameType2TimesWithoutRegistrationName_ThrowAlreadyRegisteredException()
        {
            Assert.Throws<AlreadyRegisteredTypeVInjectorException>(
            () => {
                VInjector.Register<IDummy, Dummy>();
                VInjector.Register<IDummy, Dummy>();
            });

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Failure")]
        [Fact]
        public void RegisterSameType2TimesWithSameRegistrationName_ThrowAlreadyRegisteredException()
        {
            Assert.Throws<AlreadyRegisteredTypeVInjectorException>(
            () => {
                VInjector.Register<IDummy, Dummy>(LifeTime.Global, null, 0, "SameName");
                VInjector.Register<IDummy, Dummy>(LifeTime.Global, null, 0, "SameName");
            });
            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterType_RegisterOk()
        {
            VInjector.Register<IDummy, Dummy>();

            Assert.Single(VInjector.RegistrationDictionary);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.NotNull(VInjector.RegistrationDictionary.Values.Single().Instance);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterSameType2TimesWithDifferentRegistrationName_RegisterOk()
        {
            Assert.Throws<AlreadyRegisteredTypeVInjectorException>(
            () => {
                var dummy1 = new Dummy() { Number = 1 };
                var dummy2 = new Dummy() { Number = 2 };
                VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy1);
                VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy2, 0, "DummyType2");

                Assert.Same(dummy1, VInjector.RegistrationDictionary.Values.Single(type => ((Dummy)type.Instance).Number == 1).Instance);
                Assert.Same(dummy2, VInjector.RegistrationDictionary.Values.Single(type => ((Dummy)type.Instance).Number == 2).Instance);
            });            

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterInstance_RegisterOk()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummyInstance);

            Assert.Single(VInjector.RegistrationDictionary);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.Same(dummyInstance, VInjector.RegistrationDictionary.Values.Single().Instance);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterTypeWithoutParameters_RegisterParametersOk()
        {
            VInjector.Register<IDummy, Dummy>();

            Assert.Equal(typeof(Dummy).Name, VInjector.RegistrationDictionary.Keys.Single().RegistrationName);
            Assert.Equal(LifeTime.Global, VInjector.RegistrationDictionary.Keys.Single().LifeTime);
            Assert.Equal(0, VInjector.RegistrationDictionary.Keys.Single().Priority);
            Assert.Equal(typeof(IDummy), VInjector.RegistrationDictionary.Keys.Single().InterfaceType);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.NotNull(VInjector.RegistrationDictionary.Values.Single().Instance);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterInstanceWithParameters_RegisterParametersOk()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.NewInstance, dummyInstance, 1, "MyDummyInstance");

            Assert.Single(VInjector.RegistrationDictionary);
            Assert.Equal("MyDummyInstance", VInjector.RegistrationDictionary.Keys.Single().RegistrationName);
            Assert.Equal(LifeTime.NewInstance, VInjector.RegistrationDictionary.Keys.Single().LifeTime);
            Assert.Equal(1, VInjector.RegistrationDictionary.Keys.Single().Priority);
            Assert.Equal(typeof(IDummy), VInjector.RegistrationDictionary.Keys.Single().InterfaceType);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.NotSame(dummyInstance, VInjector.RegistrationDictionary.Values.Single().Instance);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Failure")]
        [Fact]
        public void ResolveUnregisteredInstance_ThrowUnregisteredException()
        {
            Assert.Throws<UnRegisteredTypeVInjectorException>(
            () => {
                var instanceResult = VInjector.Resolve<IDummy>();
            });

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Failure")]
        [Fact]
        public void ResolveUnregisteredNamedInstance_ThrowUnregisteredException()
        {
            Assert.Throws<UnRegisteredTypeVInjectorException>(
            () => {
                VInjector.Register<IComplexDummy, ComplexDummy>();
                var instanceResult = VInjector.Resolve<IComplexDummy>("UnregisteredInstance");
            });           

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveGlobalInstance_InstanceIsTheSame()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.Same(dummyInstance, instanceResult);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveUniqueNamedGlobalInstance_InstanceIsTheSame()
        {
            var dummy1 = new Dummy { Number = 1 };
            var dummy2 = new Dummy { Number = 2 };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy1, 0, "MyDummy_1");
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummy2, 0, "MyDummy_2");

            var instanceResult = VInjector.Resolve<IDummy>("MyDummy_2");

            Assert.Same(dummy2, instanceResult);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveNewInstance_InstanceIsNotTheSame()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            //instance parameter must be ignored if NewInstance LifeTime is applied
            VInjector.Register<IDummy, Dummy>(LifeTime.NewInstance, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.NotSame(dummyInstance, instanceResult);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
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

            Assert.Same(complex, instanceResult);
            Assert.Same(dummyInstance, instanceResult.Dummy);
            Assert.Equal(111, instanceResult.Dummy.Number);
            Assert.Equal(222, instanceResult.Number);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
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

            Assert.Same(moreComplexDummy, instanceResult);
            Assert.Same(complex, instanceResult.ComplexDummy);
            Assert.Same(dummy, instanceResult.ComplexDummy.Dummy);
            Assert.Equal("1234",instanceResult.Name);
            Assert.Equal(111,instanceResult.ComplexDummy.Number);
            Assert.Equal(222,instanceResult.ComplexDummy.Dummy.Number);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveGlobalInstanceWithCyclicDependencies_InstancesAreTheSame()
        {
            var cyclic1 = new CyclicDependencyDummyPart1();
            var cyclic2 = new CyclicDependencyDummyPart2();
            VInjector.Register<ICyclicDependencyDummyPart1, CyclicDependencyDummyPart1>(LifeTime.Global, cyclic1);
            VInjector.Register<ICyclicDependencyDummyPart2, CyclicDependencyDummyPart2>(LifeTime.Global, cyclic2);

            var instanceResult = VInjector.Resolve<ICyclicDependencyDummyPart1>();
            Assert.Same(cyclic1, instanceResult);
            Assert.Same(cyclic2, instanceResult.Part2);

            VInjector.RegistrationDictionary.Clear();
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void MockVInjector_Success()
        {
            var mockVInjector = new Mock<IVInjector>() { CallBase = true };
            var dummy = new Dummy { Number = 12345678 };

            mockVInjector.Setup(injector => injector.Resolve<IDummy>(It.IsAny<string>())).Returns(dummy);

            mockVInjector.Object.Initialize<VInjectorTests>();

            var result = mockVInjector.Object.Resolve<IDummy>();

            Assert.Same(dummy, result);

            VInjector.RegistrationDictionary.Clear();
        }
    }
}
