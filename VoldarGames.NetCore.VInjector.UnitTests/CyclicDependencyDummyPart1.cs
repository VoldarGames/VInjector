using System.Diagnostics.CodeAnalysis;
using VoldarGames.NetCore.VInjector.Attributes;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    internal class CyclicDependencyDummyPart1 : ICyclicDependencyDummyPart1 
    {
        [VInject]
        public ICyclicDependencyDummyPart2 Part2 { get; set; }
    }
}