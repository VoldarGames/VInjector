namespace VInjectorTests
{
    internal interface ICyclicDependencyDummyPart1
    {
        ICyclicDependencyDummyPart2 Part2 { get; set; }
    }
}