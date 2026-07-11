using Deenote.CoreB.Architecture;

namespace Deenote
{
    public sealed class App : Application
    {
        public static App Current { get; private set; } = default!;

        public static App Create()
        {
            Current = new App();
            return Current;
        }
    }
}