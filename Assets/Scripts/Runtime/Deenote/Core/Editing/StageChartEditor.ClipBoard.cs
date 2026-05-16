#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing;
using Deenote.Editing.EditorModels.Helpers;

namespace Deenote.Core.Editing
{
    partial class StageChartEditor
    {
        private NotesClipBoard _noteClipBoard_bf = default!;
        public NotesClipBoard ClipBoard => _noteClipBoard_bf;

        private void Awake_ClipBoard()
        {
            _noteClipBoard_bf = new();
        }

        public void CopySelectedNotes()
        {
            if (_context.NoteSelection.SelectedNotes.IsEmpty)
                return;

            Placer.CancelPlaceNote();
            using var so_notes = SpanOwner<NoteData>.Allocate(_context.NoteSelection.SelectedNotes.Length);
            var notes = so_notes.Span;
            for (int i = 0; i < notes.Length; i++) {
                notes[i] = _context.NoteSelection.SelectedNotes[i].ToDataNonLinkInfo();
            }
            NoteLinkHelpers.CloneLinkInfos(_context.NoteSelection.SelectedNotes, notes);
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            _editor.RemoveSelectedNotes();
        }

        public void PasteNotes()
        {
            if (ClipBoard.Notes.IsEmpty)
                return;

            Placer.PreparePasteClipBoard();
        }
    }
}