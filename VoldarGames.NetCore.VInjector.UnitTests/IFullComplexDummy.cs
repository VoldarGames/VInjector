namespace VInjectorTests
{
    internal interface IFullComplexDummy
    {
        IMoreComplexDummy MoreComplexDummy { get; set; }

        int Number { get; set; }

        IDummy PropertyInjectDummy { get; set; }
    }
}