#nullable enable

using Deenote.Core.Editing;

namespace Deenote.Editing.Contexts
{
    internal sealed class EditorContext
    {
        public ProjectContext ProjectContext { get; }
        public NoteSelectionContext NoteSelection { get; }
        public OperationMemento Operations { get; }

        public EditorContext(ProjectContext projectContext)
        {
            ProjectContext = projectContext;
            NoteSelection = new NoteSelectionContext(this);
            Operations = new OperationMemento();
        }
    }
}
