using Deenote.Contexts.EditorModels.Helpers;

namespace Deenote.Contexts.EditorModels
{
    partial class ChartEditorModel
    {
        internal void AddNoteEditorModel(NoteEditorModel model)
        {
            NoteCollisionHelpers.UpdateCollisionsPreAdding(this, model);
            Notes.Add(model);
        }
    }
}
