using Deenote.CoreB;

namespace Deenote
{
    public partial class App : Application
    {
        public new static App Current { get; private set; } = default!;

        public App()
        {
            Current = this;
            Application.Apply(this);

            RegisterServices();
        }
    }
}