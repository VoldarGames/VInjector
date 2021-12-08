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
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();

            Assert.Equal(9, VInjector.RegistrationDictionary.Count);
        }

        [Trait("Category", "AutoRegister")]
        [Fact]
        public void AutoRegisterTypes_RetrieveOk()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();
            var instanceResult = VInjector.Resolve<IComplexDummy>();

            Assert.NotNull(instanceResult);
            Assert.NotNull(instanceResult.Dummy);            
        }

        [Trait("Category", "AutoRegister")]
        [Fact]
        public void AutoRegisterTypesWithParams_RetrieveOk()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();
            var instanceResult = VInjector.Resolve<IMoreComplexDummy>();
            Assert.NotNull(instanceResult);
            Assert.NotNull(instanceResult.ComplexDummy);
            Assert.NotNull(instanceResult.ComplexDummy.Dummy);
            Assert.Equal("MoreComplex", VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).RegistrationName);
            Assert.Equal(LifeTime.Transient, VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).LifeTime);
            Assert.Equal(1, VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).Priority);
            Assert.Equal(typeof(IMoreComplexDummy), VInjector.RegistrationDictionary.Keys.Single(type => type.InterfaceType == typeof(IMoreComplexDummy)).InterfaceType);
            Assert.Equal(typeof(MoreComplexDummy), VInjector.RegistrationDictionary.Values.Single(type => type.InstanceType == typeof(MoreComplexDummy)).InstanceType);
        }

        [Trait("Category", "Register_Failure")]
        [Fact]
        public void RegisterSameType2TimesWithoutRegistrationName_ThrowAlreadyRegisteredException()
        {
            VInjector.RegistrationDictionary.Clear();

            Assert.Throws<AlreadyRegisteredTypeVInjectorException>(
            () => {
                VInjector.Register<IDummy, Dummy>();
                VInjector.Register<IDummy, Dummy>();
            });
        }

        [Trait("Category", "Register_Failure")]
        [Fact]
        public void RegisterSameType2TimesWithSameRegistrationName_ThrowAlreadyRegisteredException()
        {
            VInjector.RegistrationDictionary.Clear();

            Assert.Throws<AlreadyRegisteredTypeVInjectorException>(
            () => {
                VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, null, 0, "SameName");
                VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, null, 0, "SameName");
            });
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterType_RegisterOk()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Register<IDummy, Dummy>();

            Assert.Single(VInjector.RegistrationDictionary);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.NotNull(VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterSameType2TimesWithDifferentRegistrationName_RegisterOk()
        {
            VInjector.RegistrationDictionary.Clear();

            var dummy1 = new Dummy() { Number = 1 };
            var dummy2 = new Dummy() { Number = 2 };
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummy1);
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummy2, 0, "DummyType2");

            Assert.Same(dummy1, VInjector.RegistrationDictionary.Values.Single(type => ((Dummy)type.Instance).Number == 1).Instance);
            Assert.Same(dummy2, VInjector.RegistrationDictionary.Values.Single(type => ((Dummy)type.Instance).Number == 2).Instance);
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterInstance_RegisterOk()
        {
            VInjector.RegistrationDictionary.Clear();
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummyInstance);

            Assert.Single(VInjector.RegistrationDictionary);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.Same(dummyInstance, VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterTypeWithoutParameters_RegisterParametersOk()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Register<IDummy, Dummy>();

            Assert.Equal(typeof(Dummy).Name, VInjector.RegistrationDictionary.Keys.Single().RegistrationName);
            Assert.Equal(LifeTime.Singleton, VInjector.RegistrationDictionary.Keys.Single().LifeTime);
            Assert.Equal(0, VInjector.RegistrationDictionary.Keys.Single().Priority);
            Assert.Equal(typeof(IDummy), VInjector.RegistrationDictionary.Keys.Single().InterfaceType);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.NotNull(VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [Trait("Category", "Register_Success")]
        [Fact]
        public void RegisterInstanceWithParameters_RegisterParametersOk()
        {
            VInjector.RegistrationDictionary.Clear();
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Transient, dummyInstance, 1, "MyDummyInstance");

            Assert.Single(VInjector.RegistrationDictionary);
            Assert.Equal("MyDummyInstance", VInjector.RegistrationDictionary.Keys.Single().RegistrationName);
            Assert.Equal(LifeTime.Transient, VInjector.RegistrationDictionary.Keys.Single().LifeTime);
            Assert.Equal(1, VInjector.RegistrationDictionary.Keys.Single().Priority);
            Assert.Equal(typeof(IDummy), VInjector.RegistrationDictionary.Keys.Single().InterfaceType);
            Assert.Equal(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.NotSame(dummyInstance, VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [Trait("Category", "Resolve_Failure")]
        [Fact]
        public void ResolveUnregisteredInstance_ThrowUnregisteredException()
        {
            VInjector.RegistrationDictionary.Clear();
            Assert.Throws<UnRegisteredTypeVInjectorException>(
            () => {
                var instanceResult = VInjector.Resolve<IDummy>();
            });
        }

        [Trait("Category", "Resolve_Failure")]
        [Fact]
        public void ResolveUnregisteredNamedInstance_ThrowUnregisteredException()
        {
            VInjector.RegistrationDictionary.Clear();
            Assert.Throws<UnRegisteredTypeVInjectorException>(
            () => {
                VInjector.Register<IComplexDummy, ComplexDummy>();
                var instanceResult = VInjector.Resolve<IComplexDummy>("UnregisteredInstance");
            });
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveGlobalInstance_InstanceIsTheSame()
        {
            VInjector.RegistrationDictionary.Clear();
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.Same(dummyInstance, instanceResult);
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveUniqueNamedGlobalInstance_InstanceIsTheSame()
        {
            VInjector.RegistrationDictionary.Clear();
            var dummy1 = new Dummy { Number = 1 };
            var dummy2 = new Dummy { Number = 2 };
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummy1, 0, "MyDummy_1");
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummy2, 0, "MyDummy_2");

            var instanceResult = VInjector.Resolve<IDummy>("MyDummy_2");

            Assert.Same(dummy2, instanceResult);
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveNewInstance_InstanceIsNotTheSame()
        {
            VInjector.RegistrationDictionary.Clear();
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            //instance parameter must be ignored if NewInstance LifeTime is applied
            VInjector.Register<IDummy, Dummy>(LifeTime.Transient, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.NotSame(dummyInstance, instanceResult);
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveGlobalInstanceWithDependencies_InstancesAreTheSame()
        {
            VInjector.RegistrationDictionary.Clear();

            var dummyInstance = new Dummy
            {
                Number = 111
            };

            var complex = new ComplexDummy
            {
                Number = 222
            };

            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummyInstance);
            VInjector.Register<IComplexDummy, ComplexDummy>(LifeTime.Singleton, complex);

            var instanceResult = VInjector.Resolve<IComplexDummy>();

            Assert.Same(complex, instanceResult);
            Assert.Same(dummyInstance, instanceResult.Dummy);
            Assert.Equal(111, instanceResult.Dummy.Number);
            Assert.Equal(222, instanceResult.Number);
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveGlobalInstanceWithNestedDependencies_InstancesAreTheSame()
        {
            VInjector.RegistrationDictionary.Clear();

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

            VInjector.Register<IMoreComplexDummy, MoreComplexDummy>(LifeTime.Singleton, moreComplexDummy);
            VInjector.Register<IComplexDummy, ComplexDummy>(LifeTime.Singleton, complex);
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummy);

            var instanceResult = VInjector.Resolve<IMoreComplexDummy>();

            Assert.Same(moreComplexDummy, instanceResult);
            Assert.Same(complex, instanceResult.ComplexDummy);
            Assert.Same(dummy, instanceResult.ComplexDummy.Dummy);
            Assert.Equal("1234",instanceResult.Name);
            Assert.Equal(111,instanceResult.ComplexDummy.Number);
            Assert.Equal(222,instanceResult.ComplexDummy.Dummy.Number);
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void ResolveGlobalInstanceWithCyclicDependencies_InstancesAreTheSame()
        {
            VInjector.RegistrationDictionary.Clear();

            var cyclic1 = new CyclicDependencyDummyPart1();
            var cyclic2 = new CyclicDependencyDummyPart2();
            VInjector.Register<ICyclicDependencyDummyPart1, CyclicDependencyDummyPart1>(LifeTime.Singleton, cyclic1);
            VInjector.Register<ICyclicDependencyDummyPart2, CyclicDependencyDummyPart2>(LifeTime.Singleton, cyclic2);

            var instanceResult = VInjector.Resolve<ICyclicDependencyDummyPart1>();
            Assert.Same(cyclic1, instanceResult);
            Assert.Same(cyclic2, instanceResult.Part2);
        }

        [Trait("Category", "Resolve_Success")]
        [Fact]
        public void MockVInjector_Success()
        {
            VInjector.RegistrationDictionary.Clear();

            var mockVInjector = new Mock<IVInjector>() { CallBase = true };
            var dummy = new Dummy { Number = 12345678 };

            mockVInjector.Setup(injector => injector.Resolve<IDummy>(It.IsAny<string>())).Returns(dummy);

            mockVInjector.Object.Initialize<VInjectorTests>();

            var result = mockVInjector.Object.Resolve<IDummy>();

            Assert.Same(dummy, result);
        }

        [Trait("Category", "AutoRegister_Ctor_Resolve_Success")]
        [Fact]
        public void ResolveNewInstanceWithGlobalCtorVInjectParameterDependencies_DependenciesAreTheSame()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();

            var dummy = VInjector.Resolve<IDummy>();
            dummy.Number = 123;
            var result = VInjector.Resolve<IComplexDummyWithCtor>("ComplexDummyWithCtorAndVInjectParameter");

            Assert.NotNull(result);
            Assert.Same(dummy, result.Dummy);
            Assert.Equal(123, result.Dummy.Number);
        }

        [Trait("Category", "AutoRegister_Ctor_Resolve_Success")]
        [Fact]
        public void ResolveNewInstanceWithGlobalCtorDependencies_DependenciesAreTheSame()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();

            var dummy = VInjector.Resolve<IDummy>();
            dummy.Number = 123;
            var result = VInjector.Resolve<IComplexDummyWithCtor>("ComplexDummyWithCtor");

            Assert.NotNull(result);
            Assert.Same(dummy, result.Dummy);
            Assert.Equal(123, result.Dummy.Number);
        }

        [Trait("Category", "AutoRegister_Ctor_Resolve_Success")]
        [Fact]
        public void ResolveNewInstanceWithGlobalCtorDependenciesAndSomePrimitives_DependenciesAreTheSameAndPrimitivesAreDefault()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();

            var result = VInjector.Resolve<IComplexDummyWithCtor>("ComplexDummyWithCtorAndPrimitives");

            Assert.NotNull(result);
            Assert.Equal(default, result.Number);
        }

        [Trait("Category", "AutoRegister_Ctor_Resolve_Success")]
        [Fact]
        public void ResolveNewInstanceWithGlobalCtorDependenciesAndSomePrimitives_DependenciesAreTheSameAndPrimitivesAreProvidedValues()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();

            var result = VInjector.Resolve<IComplexDummyWithCtor>("ComplexDummyWithCtorAndPrimitivesWithValues");

            Assert.NotNull(result);
            Assert.Equal(12, result.Number);
        }

        [Trait("Category", "AutoRegister_Ctor_Resolve_Success")]
        [Fact]
        public void ResolveNewInstanceWithGlobalCtorDependencies_NamedDependenciesAreTheSame()
        {
            VInjector.RegistrationDictionary.Clear();
            VInjector.Initialize<VInjectorTests>();

            var dummyA = new Dummy() { Number = 1 };
            var dummyB = new Dummy() { Number = 2 };

            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummyA, registrationName: "DummyA");
            VInjector.Register<IDummy, Dummy>(LifeTime.Singleton, dummyB, registrationName: "DummyB");

            var resultA = VInjector.Resolve<IComplexDummyWithCtor>("ComplexDummyWithCtorAndVInjectParameterWithNameA");
            var resultB = VInjector.Resolve<IComplexDummyWithCtor>("ComplexDummyWithCtorAndVInjectParameterWithNameB");

            Assert.NotNull(resultA);
            Assert.NotNull(resultB);

            Assert.Equal(1, resultA.Dummy.Number);
            Assert.Equal(2, resultB.Dummy.Number);

            Assert.Same(dummyA, resultA.Dummy);
            Assert.Same(dummyB, resultB.Dummy);
        }
    }
}
