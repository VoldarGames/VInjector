using VInjectorTests;
using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VoldarGames.NetCore.VInjector.UnitTests
{
    [VAutoRegister(typeof(IComplexDummyWithCtor), LifeTime.NewInstance, 0, "ComplexDummyWithCtorAndPrimitives")]
    internal class ComplexDummyWithCtorAndPrimitives : IComplexDummyWithCtor
    {
        [VInjectCtor]
        public ComplexDummyWithCtorAndPrimitives(IDummy dummy, int number)
        {
            Dummy = dummy;
            Number = number;
        }

        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}