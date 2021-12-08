using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VInjectorTests
{
    [VAutoRegister(typeof(IComplexDummyWithCtor), LifeTime.NewInstance, 0, "ComplexDummyWithCtorAndVInjectParameterWithNameA")]
    internal class ComplexDummyWithCtorAndVInjectParameterWithNameA : IComplexDummyWithCtor
    {
        public ComplexDummyWithCtorAndVInjectParameterWithNameA()
        {
        }
        
        [VInjectCtor]
        public ComplexDummyWithCtorAndVInjectParameterWithNameA([VInjectParameter("DummyA")] IDummy dummy)
        {
            Dummy = dummy;
        }

        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}