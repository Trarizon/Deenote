#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;

namespace Deenote.Editing.EditorModels
{
    internal sealed class WarningNoteEditorModel : INoteLink
    {
        public uint Uid { get; }
        public float Time { get; set; }

        public WarningNoteEditorModel(WarningNoteModel model)
        {
            Uid = INoteUnique.GetUid();
            Time = model.Time;
        }

        public INoteLink? NextLink { get; set; }
        public INoteLink? PrevLink { get; set; }

        float INoteLocation.Position => NoteConstraints.DefaultWarningNotePosition;

        float INoteSpeed.Speed => NoteConstraints.DefaultSpeed;

        public WarningNoteModel ToModelNonLinkInfo()
        {
            return new WarningNoteModel(Time);
        }
    }
}
