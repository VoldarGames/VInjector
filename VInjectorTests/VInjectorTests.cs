using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VInjectorCore;

namespace VInjectorTests
{
    [TestClass]
    public class VInjectorTests
    {
        [TestCleanup]
        public void TearDown()
        {
            VInjector.RegistrationDictionary.Clear();    
        }

        [TestCategory("Register")]
        [TestMethod]
        public void RegisterType_RegisterOk()
        {
            VInjector.Register<IDummy, Dummy>();

            Assert.AreEqual(1, VInjector.RegistrationDictionary.Count);
            Assert.AreEqual(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.IsNotNull(VInjector.RegistrationDictionary.Values.Single().Instance);
        }

        [TestCategory("Register")]
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

        [TestCategory("Register")]
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

        [TestCategory("Register")]
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

        [TestCategory("Retrieve")]
        [TestMethod]
        public void RetrieveGlobalInstance_InstanceIsTheSame()
        {
            var dummyInstance = new Dummy
            {
                Number = 123
            };
            VInjector.Register<IDummy, Dummy>(LifeTime.Global, dummyInstance);

            var instanceResult = VInjector.Resolve<IDummy>();

            Assert.AreSame(dummyInstance, instanceResult);
        }

        [TestCategory("Retrieve")]
        [TestMethod]
        public void RetrieveNewInstance_InstanceIsNotTheSame()
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
    }
}
