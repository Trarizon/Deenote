#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Contexts;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Editing.Operations;
using Deenote.Editing.Operations.Components;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Deenote.Editing
{
    public sealed class ChartNotesEditor
    {
        private static readonly ImmutableArray<PianoSoundData> _defaultNoteSounds = ImmutableArray.Create(new PianoSoundData(0f, 0f, 72, 0));

        private readonly ProjectContext _context;
        private readonly EditorContext _editor;

        internal ChartNotesEditor(EditorContext editor, ProjectContext project)
        {
            _context = project;
            _editor = editor;
        }

        public void AddNote(NoteData note)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart.GetAddNoteOperation(new NoteEditorModel(note))
                .OnRedone(note =>
                {
                    _editor.NoteSelection.ClearSelection();
                    ModelAsserts.AssertChartEditorModel(_context.CurrentChart);
                }));
        }

        public void AddNotes(ReadOnlySpan<NoteData> notes)
        {
            if (_context.CurrentChart is null)
                return;
            if (notes.IsEmpty)
                return;

            var array = new NoteEditorModel[notes.Length];
            for (var i = 0; i < notes.Length; i++)
                array[i] = new NoteEditorModel(notes[i]);

            var wrap = ImmutableCollectionsMarshal.AsImmutableArray(array);

            _editor.Operations.Do(_context.CurrentChart.GetAddNotesOperation(wrap.ToImmutableArray())
                .OnRedone(notes =>
                {
                    _editor.NoteSelection.ReselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_context.CurrentChart);
                })
                .OnUndone(notes =>
                {
                    _editor.NoteSelection.DeselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_context.CurrentChart);
                }));
        }

        public void RemoveNotes(ImmutableArray<NoteEditorModel> notes)
        {
            if (_context.CurrentChart is null)
                return;
            if (notes.IsEmpty)
                return;

            _editor.Operations.Do(_context.CurrentChart.GetRemoveNotesOperation(notes.ToImmutableArray())
                .OnRedone(notes =>
                {
                    _editor.NoteSelection.DeselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_context.CurrentChart);
                }));
        }

        public void RemoveSelectedNotes()
        {
            if (_context.CurrentChart is null)
                return;
            if (_editor.NoteSelection.SelectedNotes.IsEmpty)
                return;

            var notes = _editor.NoteSelection.SelectedNotes.ToImmutableArray();

            _editor.Operations.Do(_context.CurrentChart.GetRemoveNotesOperation(notes.ToImmutableArray())
                .OnRedone(notes =>
                {
                    _editor.NoteSelection.ClearSelection();
                    ModelAsserts.AssertChartEditorModel(_context.CurrentChart);
                })
                .OnUndone(notes =>
                {
                    _editor.NoteSelection.ReselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_context.CurrentChart);
                }));
        }

        // Notes properties

        public void EditPositionCoord(ReadOnlySpan<NoteEditorModel> notes, Func<NoteCoord, NoteCoord> valueSelector)
        {
            if (_context.CurrentProject is null)
                return;
            if (_context.CurrentChart is null)
                return;

            float clipLength = _context.CurrentProject.AudioLength ?? float.MaxValue;
            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesCoordOperation(notes.ToImmutableArray(), v => NoteCoord.Clamp(valueSelector(v), clipLength)));
        }

        public void EditNotesTime(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_context.CurrentChart is null)
                return;

            float clipLength = _context.CurrentProject?.AudioLength ?? float.MaxValue;
            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesTimeOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampTime(valueSelector(v), clipLength)));
        }

        public void EditNotesTime(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            float clipLength = _context.CurrentProject?.AudioLength ?? float.MaxValue;
            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesTimeOperation(notes.ToImmutableArray(), NoteConstraints.ClampTime(value, clipLength)));
        }

        public void EditNotesPosition(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesPositionOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampPosition(valueSelector(v))));
        }

        public void EditNotesPosition(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesPositionOperation(notes.ToImmutableArray(), NoteConstraints.ClampPosition(value)));
        }

        public void EditNotesSize(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampSize(valueSelector(v)), n => n.Size, (n, v) => n.Size = v));
        }

        public void EditNotesSize(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), NoteConstraints.ClampSize(value), n => n.Size, (n, v) => n.Size = v));
        }

        public void EditNotesShift(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), value, n => n.Shift, (n, v) => n.Shift = v));
        }

        public void EditNotesSpeed(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampSpeed(valueSelector(v)), n => n.Speed, (n, v) => n.Speed = v));
        }

        public void EditNotesSpeed(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), NoteConstraints.ClampSpeed(value), n => n.Speed, (n, v) => n.Speed = v));
        }

        public void EditNotesDuration(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesDurationOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampDuration(valueSelector(v))));
        }

        public void EditNotesDuration(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesDurationOperation(notes.ToImmutableArray(), NoteConstraints.ClampDuration(value)));
        }

        public void EditNotesEndTime(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesEndTimeOperation(notes.ToImmutableArray(), valueSelector));
        }

        public void EditNotesEndTime(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesEndTimeOperation(notes.ToImmutableArray(), value));
        }

        public void EditNotesVibrate(ReadOnlySpan<NoteEditorModel> notes, bool value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), value, n => n.Vibrate, (n, v) => n.Vibrate = v));
        }

        public void EditNotesKind(ReadOnlySpan<NoteEditorModel> notes, NoteKind value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesKindOperation(notes.ToImmutableArray(), value));
        }

        public void EditNotesWarningType(ReadOnlySpan<NoteEditorModel> notes, WarningType value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), value, n => n.WarningType, (n, v) => n.WarningType = v));
        }

        public void EditNotesEventId(ReadOnlySpan<NoteEditorModel> notes, string value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), value, n => n.EventId, (n, v) => n.EventId = v));
        }

        public void EditNotesSounds(ReadOnlySpan<NoteEditorModel> notes, ImmutableArray<PianoSoundData> value)
        {
            if (_context.CurrentChart is null)
                return;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesSoundsOperation(notes.ToImmutableArray(), value.ToImmutableArray()));
        }

        public void EditNotesSounds(ReadOnlySpan<NoteEditorModel> notes, bool hasSound)
        {
            if (_context.CurrentChart is null)
                return;

            using var so_editNotes = SpanOwner<NoteEditorModel>.Allocate(notes.Length);
            var span = so_editNotes.Span;
            int idx = 0;
            foreach (var note in notes) {
                if (note.HasSounds != hasSound)
                    span[idx++] = note;
            }

            var sounds = hasSound ? _defaultNoteSounds : ImmutableArray<PianoSoundData>.Empty;

            _editor.Operations.Do(_context.CurrentChart
                .GetEditNotesSoundsOperation(span.ToImmutableArray(), sounds.ToImmutableArray()));
        }

        public void CreateHoldBetween(NoteEditorModel head, NoteEditorModel tail)
        {
            if (_context.CurrentChart is null)
                return;

            if (tail.Time == head.Time)
                return;
            if (tail.Time <= head.Time)
                (head, tail) = (tail, head);

            var duration = tail.Time - head.Time;
            var rmv = _context.CurrentChart.GetRemoveNotesOperation(ImmutableArray.Create(tail));
            var edit = _context.CurrentChart.GetEditNotesDurationOperation(ImmutableArray.Create(head), duration);
            _editor.Operations.Do(new CombinedOperation(rmv, edit));
        }

        public void InsertTempo(TempoRange range)
        {
            if (_context.CurrentProject is null)
                return;

            _editor.Operations.Do(_context.CurrentProject
                .InsertTempo(range));
        }
    }
}
