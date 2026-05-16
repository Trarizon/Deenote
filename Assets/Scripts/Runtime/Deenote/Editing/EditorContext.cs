#nullable enable

using Deenote.Contexts;
using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.Editing.Grids;
using Deenote.Editing.NotePlacement;
using Deenote.Editing.NoteSelection;

namespace Deenote.Editing
{
    public sealed class EditorContext
    {
        public ProjectContext ProjectContext { get; }
        public NoteSelectionContext NoteSelection { get; }
        public NotePlacementContext NotePlacement { get; }
        public OperationMemento Operations { get; }
        public GridsContext Grids { get; }
        public NotesClipBoard ClipBoard { get; }

        public EditorContext(ProjectContext projectContext, SaveSystem storage)
        {
            ProjectContext = projectContext;
            NoteSelection = new NoteSelectionContext(this);
            NotePlacement = new NotePlacementContext();
            Operations = new OperationMemento();
            Grids = new GridsContext(projectContext, storage);
        }
    }
}
