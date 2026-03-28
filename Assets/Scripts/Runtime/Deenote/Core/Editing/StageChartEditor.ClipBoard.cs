#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.CoreB.Models.Notes;
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
            if (Selector.SelectedNotes.IsEmpty)
                return;

            Placer.CancelPlaceNote();
            using var so_notes = SpanOwner<NoteData>.Allocate(Selector.SelectedNotes.Length);
            var notes = so_notes.Span;
            for (int i = 0; i < notes.Length; i++) {
                notes[i] = Selector.SelectedNotes[i].ToDataNonLinkInfo();
            }
            NoteLinkHelpers.CloneLinkInfos(Selector.SelectedNotes, notes);
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            RemoveNotes(Selector.SelectedNotes);
        }

        public void PasteNotes()
        {
            if (ClipBoard.Notes.IsEmpty)
                return;

            Placer.PreparePasteClipBoard();
        }
    }
}