namespace VInjectorTests
{
    internal interface IComplexDummyWithCtor
    {
        IDummy Dummy { get; set; }

        int Number { get; set; }
    }
}