namespace Deenote.CoreB
{
    public abstract class Application
    {
        public static Application Current { get; private set; } = default!;

        public Deenote.Core.Logging.Logger Logger { get; } = new();

        protected static void Apply(Application application)
        {
            Current = application;
        }

        internal static void OnDomainReloaded()
        {
            Current = null!;
        }
    }
}