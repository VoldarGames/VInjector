using System.Diagnostics.CodeAnalysis;
using VoldarGames.NetCore.VInjector.Attributes;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    internal class CyclicDependencyDummyPart2 : ICyclicDependencyDummyPart2
    {
        [VInject]
        public ICyclicDependencyDummyPart1 Part1 { get; set; }
    }
}