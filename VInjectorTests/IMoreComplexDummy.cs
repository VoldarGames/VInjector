using System;

namespace VInjectorTests
{
    internal interface IMoreComplexDummy
    {
        IComplexDummy ComplexDummy { get; set; }

        string Name { get; set; }
    }
}