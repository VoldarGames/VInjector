namespace VoldarGames.NetCore.VInjector.Core.Interfaces
{
    public interface IVInjector
    {
        void Initialize<TVAppContext>()
            where TVAppContext : IVAppContext;

        void Register<TInterface, TInstance>(LifeTime lifeTime = LifeTime.Singleton, TInstance instance = default(TInstance), int priority = 0, string registrationName = null)
            where TInstance : class, TInterface, new()
            where TInterface : class;

        TInterface Resolve<TInterface>(string registrationName = null) where TInterface : class;
    }
}
