using System.Diagnostics.CodeAnalysis;
using VoldarGames.NetCore.VInjector.Attributes;
using VoldarGames.NetCore.VInjector.Core;

namespace VInjectorTests
{
    [ExcludeFromCodeCoverage]
    [VAutoRegister(typeof(IFullComplexDummy), LifeTime.Transient, 0, "FullComplexDummy")]
    internal class FullComplexDummy : IFullComplexDummy
    {
        [VInjectCtor]
        public FullComplexDummy([VInjectParameter]IMoreComplexDummy moreComplexDummy)
        {
            MoreComplexDummy = moreComplexDummy;
        }

        public IMoreComplexDummy MoreComplexDummy { get; set; }
        public int Number { get; set; }

        [VInject("DummyA")]
        public IDummy PropertyInjectDummy { get; set; }
    }
}