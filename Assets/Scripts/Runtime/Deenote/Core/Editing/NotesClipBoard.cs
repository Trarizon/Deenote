#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library.Collections;
using System;
using UnityEngine.Pool;

namespace Deenote.Core.Editing
{
    public sealed class NotesClipBoard
    {
        private PooledObjectListView<NoteData> _notes;

        public ReadOnlySpan<NoteData> Notes => _notes.AsSpan();

        public NoteCoord BaseCoord => _notes.Count > 0 ? _notes[0].PositionCoord : new(0f, 0f);

        public NotesClipBoard()
        {
            _notes = new(new ObjectPool<NoteData>(() => new NoteData()));
        }

        public void SetNotes(ReadOnlySpan<NoteData> notes)
        {
            using (var resetter = _notes.Resetting(notes.Length)) {
                foreach (var note in notes) {
                    resetter.Add(out var cnote);
                    note.CloneToNonLinkInfo(cnote);
                }
            }
            NoteLinkHelpers.CloneLinkInfos(notes, _notes.AsSpan());
        }
    }
}