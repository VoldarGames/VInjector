using System.Diagnostics.CodeAnalysis;
using VInjectorCore.Attributes;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    internal class CyclicDependencyDummyPart1 : ICyclicDependencyDummyPart1 
    {
        [VInject]
        public ICyclicDependencyDummyPart2 Part2 { get; set; }
    }
}