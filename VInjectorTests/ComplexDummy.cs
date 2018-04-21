using System.Diagnostics.CodeAnalysis;
using VInjectorCore.Attributes;
using VInjectorCore.Core;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [VAutoRegister(typeof(IComplexDummy), LifeTime.Global, 0, "MyComplexDummy")]
    internal class ComplexDummy : IComplexDummy
    {
        [VInject]
        public Dummy Dummy { get; set; }
        public int Number { get; set; }
    }
}