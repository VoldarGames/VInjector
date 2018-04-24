using System.Diagnostics.CodeAnalysis;
using VInjectorCore.Attributes;
using VInjectorCore.Core;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [VAutoRegister(typeof(IMoreComplexDummy), LifeTime.NewInstance, 1, "MoreComplex")]
    internal class MoreComplexDummy : IMoreComplexDummy 
    {
        [VInject]
        public IComplexDummy ComplexDummy { get; set; }
        public string Name { get; set; }
    }
}