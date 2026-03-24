#nullable enable

using Deenote.Editing.Contexts;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using System;

namespace Deenote.Core.Editing
{
    public sealed class ChartNotesEditor
    {
        private readonly EditorContext _context;

        private bool _enabled_bf;
        public bool IsEnabled
        {
            get => _enabled_bf && _context.ProjectContext.CurrentChart is not null;
        }

        public void AddNote(NoteModel note, Action<NoteModel>? onRedone)
        {
            if (_context.ProjectContext.CurrentChart is null)
                return;

            var operation = GetAddNoteOperation(note);
            if (onRedone is not null) {
                operation.OnRedone(onRedone);
            }

            operation.OnUndone(note => _context.NoteSelection.DeselectNote(note));
            _context.Operations.Do(operation);
        }

        private ModelOperations.NotifiableOperation<NoteModel> GetAddNoteOperation(NoteModel note)
        {
            var chart = _context.ProjectContext.CurrentChart!;
            return chart.AddNote(note);
                //.OnRedone(note =>
                //{
                //    _context.ProjectContext.RaiseChartNotesChanged(_context.ProjectContext.CurrentChart)
                //    NodeTimeComparer.AssertInOrder(_context.CurrentChart!.NoteNodes);
                //})
                //.OnUndone(_ =>
                //{
                //    _selector.DeselectNote(note);
                //    _context.RaiseNoteCollectionChanged(new(_context.CurrentChart!));
                //    NodeTimeComparer.AssertInOrder(_context.CurrentChart!.NoteNodes);
                //});
        }

    }
}
