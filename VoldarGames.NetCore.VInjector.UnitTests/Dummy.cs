using System.Diagnostics.CodeAnalysis;
using VoldarGames.NetCore.VInjector.Attributes;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [VAutoRegister(typeof(IDummy))]
    internal class Dummy : IDummy
    {
        public int Number { get; set; }
    }
}