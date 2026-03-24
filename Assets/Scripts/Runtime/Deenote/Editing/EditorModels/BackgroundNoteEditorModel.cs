#nullable enable

using Deenote.CoreB.Models.Notes;
using Deenote.Library.Collections;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    internal sealed class BackgroundNoteEditorModel : INoteLinkNode
    {
        public float Time { get; set; }
        public List<PianoSoundData> Sounds { get; } = new();

        public BackgroundNoteEditorModel(BackgroundNoteModel model)
        {
            Time = model.Time;
            Sounds.AddRange(model.Sounds.AsSpan());
        }

        public INoteLinkNode? NextLink { get; set; }
        public INoteLinkNode? PrevLink { get; set; }

        public BackgroundNoteModel ToModelNonLinkInfo()
        {
            return new BackgroundNoteModel(Time, Sounds.AsSpan());
        }
    }
}
