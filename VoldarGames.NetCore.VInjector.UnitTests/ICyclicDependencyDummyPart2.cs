namespace VInjectorTests
{
    internal interface ICyclicDependencyDummyPart2
    {
        ICyclicDependencyDummyPart1 Part1 { get; set; }
    }
}