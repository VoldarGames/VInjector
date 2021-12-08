using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VInjectorTests
{
    [VAutoRegister(typeof(IComplexDummyWithCtor), LifeTime.NewInstance, 0, "ComplexDummyWithCtor")]
    internal class ComplexDummyWithCtor : IComplexDummyWithCtor
    {
        public ComplexDummyWithCtor()
        {
        }

        [VInjectCtor]
        public ComplexDummyWithCtor(IDummy dummy)
        {
            Dummy = dummy;
        }

        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}