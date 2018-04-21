using System.Diagnostics.CodeAnalysis;

namespace VInjectorTests
{
    internal interface IDummy
    {
        int Number { get; set; }
    }

    internal interface IComplexDummy
    {
        Dummy Dummy { get; set; }
        int Number { get; set; }
    }
}