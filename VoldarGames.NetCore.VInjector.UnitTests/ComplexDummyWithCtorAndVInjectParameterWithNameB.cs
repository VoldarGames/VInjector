using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VInjectorTests
{
    [VAutoRegister(typeof(IComplexDummyWithCtor), LifeTime.Transient, 0, "ComplexDummyWithCtorAndVInjectParameterWithNameB")]
    internal class ComplexDummyWithCtorAndVInjectParameterWithNameB : IComplexDummyWithCtor
    {
        public ComplexDummyWithCtorAndVInjectParameterWithNameB()
        {
        }
        
        [VInjectCtor]
        public ComplexDummyWithCtorAndVInjectParameterWithNameB([VInjectParameter("DummyB")] IDummy dummy)
        {
            Dummy = dummy;
        }

        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}