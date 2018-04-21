using System.Diagnostics.CodeAnalysis;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    internal class Dummy : IDummy
    {
        public int Number { get; set; }
    }
}