using Deenote.Contexts;

namespace Deenote
{
    partial class App
    {
        public ProjectContext ProjectContext { get; private set; } = default!;

        private void RegisterServices()
        {
            ProjectContext = new();
        }
    }
}