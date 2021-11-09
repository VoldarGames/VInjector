using System.Diagnostics.CodeAnalysis;
using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [VAutoRegister(typeof(IComplexDummy), LifeTime.Global, 0, "MyComplexDummy")]
    internal class ComplexDummy : IComplexDummy
    {
        [VInject]
        public IDummy Dummy { get; set; }
        public int Number { get; set; }
    }
}