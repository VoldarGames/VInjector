using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VInjectorTests
{
    [VAutoRegister(typeof(IComplexDummyWithCtor), LifeTime.NewInstance, 0, "ComplexDummyWithCtorAndVInjectParameter")]
    internal class ComplexDummyWithCtorAndVInjectParameter : IComplexDummyWithCtor
    {
        public ComplexDummyWithCtorAndVInjectParameter()
        {
        }
        
        [VInjectCtor]
        public ComplexDummyWithCtorAndVInjectParameter([VInjectParameter] IDummy dummy)
        {
            Dummy = dummy;
        }

        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}