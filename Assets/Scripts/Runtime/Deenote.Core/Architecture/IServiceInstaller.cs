namespace Deenote.CoreB.Architecture
{
    public interface IServiceInstaller<TApp> where TApp : Application
    {
        void RegisterServices(TApp app);
    }
}
