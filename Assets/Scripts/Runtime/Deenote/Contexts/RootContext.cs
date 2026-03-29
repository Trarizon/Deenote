#nullable enable

using Deenote.Core;

namespace Deenote.Contexts
{
    public sealed class RootContext
    {
        public ProjectContext Project { get; }
        internal EditorContext Editor { get; }
        public EnvironmentContext Environment { get; }

        internal RootContext(SaveSystem saveSystem)
        {
            Project = new ProjectContext();
            Editor = new EditorContext(Project);
            Environment = new EnvironmentContext(saveSystem);
        }
    }
}
