#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Deenote.Editing
{
    // OPTIMIZE: Pool
    public sealed class NotesClipBoard
    {
        private readonly List<NoteData> _notes=new();

        public ReadOnlySpan<NoteData> Notes => _notes.AsSpan();

        public NoteCoord BaseCoord => _notes.Count > 0 ? _notes[0].PositionCoord : new(0f, 0f);

        internal NotesClipBoard()
        {
        }

        public void SetNotes(ReadOnlySpan<NoteData> notes)
        {
            _notes.Clear();
            for(int i = 0; i < notes.Length; i++) {
                var n= notes[i].CloneNonLinkInfo();
                _notes.Add(n);
            }
            NoteLinkHelpers.CloneLinkInfos(notes, _notes.AsSpan());
        }
    }
}