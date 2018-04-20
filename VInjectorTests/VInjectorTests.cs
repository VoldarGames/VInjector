using System;
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

        [TestMethod]
        public void RegisterType_RegisterOk()
        {
            VInjector.Register<IDummy, Dummy>();

            Assert.AreEqual(1, VInjector.RegistrationDictionary.Count);
            Assert.AreEqual(typeof(Dummy), VInjector.RegistrationDictionary.Values.Single().InstanceType);
            Assert.IsNotNull(VInjector.RegistrationDictionary.Values.Single().Instance);
        }

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
    }
}
