using Deenote.CoreB;
using Deenote.EditorModels;
using Deenote.Entities;
using Deenote.Entities.Models;
using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Deenote.Editing
{
    public sealed class NotesClipBoard2
    {
        private ObjectPool<NotePrototypeModel> _pool;

        private List<NotePrototypeModel> _notes = new();
        private NoteCoord _baseCoord;

        public ReadOnlySpan<NotePrototypeModel> Notes => _notes.AsSpan();
        public NoteCoord BaseCoord => _baseCoord;

        public NotesClipBoard2()
        {
            _pool = new ObjectPool<NotePrototypeModel>(
                () => new NotePrototypeModel(),
                collectionCheck: false
            );
        }

        public void SetClipBoard(ReadOnlySpan<NotePrototypeModel> notes, NoteCoord baseCoord)
        {
            _baseCoord = baseCoord;
            _notes.Replace(notes);
        }

        public void SetClipBoard(ReadOnlySpan<NoteModel> notes, NoteCoord? baseCoord = null)
        {
            if (notes.IsEmpty) {
                return;
            }

            _baseCoord = baseCoord ?? notes[0].PositionCoord;

            _notes.Clear();
            foreach (var note in notes) {
                var prototype = _pool.Get();
                prototype.FromDataNonLinkInfo(note, _baseCoord);
                _notes.Add(prototype);
            }
            NoteLinkHelpers.CloneLinkInfos(notes, _notes.AsSpan());
        }
    }
}
