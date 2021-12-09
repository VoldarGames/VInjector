using VInjectorTests;
using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VoldarGames.NetCore.VInjector.UnitTests
{
    [VAutoRegister(typeof(IComplexDummyWithCtor), LifeTime.Transient, 0, "ComplexDummyWithCtorAndPrimitivesWithValues")]
    internal class ComplexDummyWithCtorAndPrimitivesWithValues : IComplexDummyWithCtor
    {
        [VInjectCtor]
        public ComplexDummyWithCtorAndPrimitivesWithValues(IDummy dummy, [VInjectParameter(defaultValue: 12)]int number)
        {
            Dummy = dummy;
            Number = number;
        }

        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}