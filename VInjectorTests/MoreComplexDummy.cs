using System.Diagnostics.CodeAnalysis;
using VInjectorCore.Attributes;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    internal class MoreComplexDummy : IMoreComplexDummy 
    {
        [VInject]
        public IComplexDummy ComplexDummy { get; set; }
        public string Name { get; set; }
    }
}