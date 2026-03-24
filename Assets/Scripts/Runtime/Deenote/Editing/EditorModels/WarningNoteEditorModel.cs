#nullable enable

using Deenote.CoreB.Models.Notes;

namespace Deenote.Editing.EditorModels
{
    internal sealed class WarningNoteEditorModel : INoteLinkNode
    {
        public float Time { get; set; }

        public WarningNoteEditorModel(WarningNoteModel model)
        {
            Time = model.Time;
        }

        public INoteLinkNode? NextLink { get; set; }
        public INoteLinkNode? PrevLink { get; set; }

        public WarningNoteModel ToModelNonLinkInfo()
        {
            return new WarningNoteModel(Time);
        }
    }
}
