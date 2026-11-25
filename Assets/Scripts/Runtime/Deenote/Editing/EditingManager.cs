using Deenote.Core;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Project;

namespace Deenote.Editing;

public sealed class EditingManager
{
    public ChartNotesSelector NotesSelector { get; }
    internal StageDragSelector DragSelector { get; }
    internal StageNotesPlacer2 NotesPlacer { get; }

    internal EditingManager(ChartNotesSelector notesSelector, StageDragSelector dragSelector, StageNotesPlacer2 notesPlacer)
    {
        NotesSelector = notesSelector;
        DragSelector = dragSelector;
        NotesPlacer = notesPlacer;
    }
}
