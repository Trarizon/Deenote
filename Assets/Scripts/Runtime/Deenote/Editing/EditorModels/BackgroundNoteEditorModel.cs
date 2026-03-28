#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Library.Collections;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    internal sealed class BackgroundNoteEditorModel : INoteLink
    {
        public uint Uid { get; }
        public float Time { get; set; }
        public List<PianoSoundData> Sounds { get; } = new();

        public BackgroundNoteEditorModel(BackgroundNoteModel model)
        {
            Uid = INoteUnique.GetUid();
            Time = model.Time;
            Sounds.AddRange(model.Sounds.AsSpan());
        }

        public INoteLink? NextLink { get; set; }
        public INoteLink? PrevLink { get; set; }

        float INoteLocation.Position => NoteConstraints.DefaultBackgroundPosition;

        float INoteSpeed.Speed => NoteConstraints.DefaultSpeed;

        public BackgroundNoteModel ToModelNonLinkInfo()
        {
            return new BackgroundNoteModel(Time, Sounds.AsSpan());
        }
    }
}
