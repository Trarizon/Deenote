#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Contexts;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Assertions;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Editing.Operations;
using Deenote.Editing.Operations.Components;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Deenote.Editing
{
    public sealed partial class ChartNotesEditor
    {
        private readonly ProjectContext _project;
        private readonly EditorContext _context;

        internal ChartNotesEditor(EditorContext editor, ProjectContext project)
        {
            _project = project;
            _context = editor;
        }

        #region Add Remove

        public void AddNote(NoteData note)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart.GetAddNoteOperation(new NoteEditorModel(note))
                .OnRedone(note =>
                {
                    _context.NoteSelection.ClearSelection();
                    ModelAsserts.AssertChartEditorModel(_project.CurrentChart);
                }));
        }

        public void AddNotes(ReadOnlySpan<NoteData> notes)
        {
            if (_project.CurrentChart is null)
                return;
            if (notes.IsEmpty)
                return;

            var array = new NoteEditorModel[notes.Length];
            for (var i = 0; i < notes.Length; i++)
                array[i] = new NoteEditorModel(notes[i]);

            var wrap = ImmutableCollectionsMarshal.AsImmutableArray(array);

            _context.Operations.Do(_project.CurrentChart.GetAddNotesOperation(wrap.ToImmutableArray())
                .OnRedone(notes =>
                {
                    _context.NoteSelection.ReselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_project.CurrentChart);
                })
                .OnUndone(notes =>
                {
                    _context.NoteSelection.DeselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_project.CurrentChart);
                }));
        }

        public void RemoveNotes(ImmutableArray<NoteEditorModel> notes)
        {
            if (_project.CurrentChart is null)
                return;
            if (notes.IsEmpty)
                return;

            _context.Operations.Do(_project.CurrentChart.GetRemoveNotesOperation(notes.ToImmutableArray())
                .OnRedone(notes =>
                {
                    _context.NoteSelection.DeselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_project.CurrentChart);
                }));
        }

        public void RemoveSelectedNotes()
        {
            if (_project.CurrentChart is null)
                return;
            if (_context.NoteSelection.SelectedNotes.IsEmpty)
                return;

            var notes = _context.NoteSelection.SelectedNotes.ToImmutableArray();

            _context.Operations.Do(_project.CurrentChart.GetRemoveNotesOperation(notes.ToImmutableArray())
                .OnRedone(notes =>
                {
                    _context.NoteSelection.ClearSelection();
                    ModelAsserts.AssertChartEditorModel(_project.CurrentChart);
                })
                .OnUndone(notes =>
                {
                    _context.NoteSelection.ReselectNotes(notes.AsSpan());
                    ModelAsserts.AssertChartEditorModel(_project.CurrentChart);
                }));
        }

        #endregion

        // Notes properties

        #region Edit Notes' properties

        public void EditNotesCoord(ReadOnlySpan<NoteEditorModel> notes, Func<NoteCoord, NoteCoord> valueSelector)
        {
            if (_project.CurrentProject is null)
                return;
            if (_project.CurrentChart is null)
                return;

            float clipLength = _project.CurrentProject.AudioLength ?? float.MaxValue;
            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesCoordOperation(notes.ToImmutableArray(), v => NoteCoord.Clamp(valueSelector(v), clipLength)));
        }

        public void EditNotesTime(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_project.CurrentChart is null)
                return;

            float clipLength = _project.CurrentProject?.AudioLength ?? float.MaxValue;
            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesTimeOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampTime(valueSelector(v), clipLength)));
        }

        public void EditNotesTime(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            float clipLength = _project.CurrentProject?.AudioLength ?? float.MaxValue;
            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesTimeOperation(notes.ToImmutableArray(), NoteConstraints.ClampTime(value, clipLength)));
        }

        public void EditNotesPosition(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesPositionOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampPosition(valueSelector(v))));
        }

        public void EditNotesPosition(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesPositionOperation(notes.ToImmutableArray(), NoteConstraints.ClampPosition(value)));
        }

        public void EditNotesSize(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.Size), v => NoteConstraints.ClampSize(valueSelector(v)), n => n.Size, (n, v) => n.Size = v));
        }

        public void EditNotesSize(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.Size), NoteConstraints.ClampSize(value), n => n.Size, (n, v) => n.Size = v));
        }

        public void EditNotesShift(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.Shift), value, n => n.Shift, (n, v) => n.Shift = v));
        }

        public void EditNotesSpeed(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.Speed), v => NoteConstraints.ClampSpeed(valueSelector(v)), n => n.Speed, (n, v) => n.Speed = v));
        }

        public void EditNotesSpeed(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.Speed), NoteConstraints.ClampSpeed(value), n => n.Speed, (n, v) => n.Speed = v));
        }

        public void EditNotesDuration(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesDurationOperation(notes.ToImmutableArray(), v => NoteConstraints.ClampDuration(valueSelector(v))));
        }

        public void EditNotesDuration(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesDurationOperation(notes.ToImmutableArray(), NoteConstraints.ClampDuration(value)));
        }

        public void EditNotesEndTime(ReadOnlySpan<NoteEditorModel> notes, Func<float, float> valueSelector)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesEndTimeOperation(notes.ToImmutableArray(), valueSelector));
        }

        public void EditNotesEndTime(ReadOnlySpan<NoteEditorModel> notes, float value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesEndTimeOperation(notes.ToImmutableArray(), value));
        }

        public void EditNotesVibrate(ReadOnlySpan<NoteEditorModel> notes, bool value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.Vibrate), value, n => n.Vibrate, (n, v) => n.Vibrate = v));
        }

        public void EditNotesKind(ReadOnlySpan<NoteEditorModel> notes, NoteKind value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesKindOperation(notes.ToImmutableArray(), value));
        }

        public void EditNotesWarningType(ReadOnlySpan<NoteEditorModel> notes, WarningType value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.WarningType), value, n => n.WarningType, (n, v) => n.WarningType = v));
        }

        public void EditNotesEventId(ReadOnlySpan<NoteEditorModel> notes, string value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesOperation(notes.ToImmutableArray(), nameof(NoteEditorModel.EventId), value, n => n.EventId, (n, v) => n.EventId = v));
        }

        public void EditNotesSounds(ReadOnlySpan<NoteEditorModel> notes, ImmutableArray<PianoSoundData> value)
        {
            if (_project.CurrentChart is null)
                return;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesSoundsOperation(notes.ToImmutableArray(), value.ToImmutableArray()));
        }

        public void EditNotesSounds(ReadOnlySpan<NoteEditorModel> notes, bool hasSound)
        {
            if (_project.CurrentChart is null)
                return;

            using var so_editNotes = SpanOwner<NoteEditorModel>.Allocate(notes.Length);
            var span = so_editNotes.Span;
            int idx = 0;
            foreach (var note in notes) {
                if (note.HasSounds != hasSound)
                    span[idx++] = note;
            }

            var sounds = hasSound ? NoteSoundsHelpers.EditorDefaultSounds : ReadOnlySpan<PianoSoundData>.Empty;

            _context.Operations.Do(_project.CurrentChart
                .GetEditNotesSoundsOperation(span.ToImmutableArray(), sounds.ToImmutableArray()));
        }

        #endregion

        public void CreateHoldBetween(NoteEditorModel head, NoteEditorModel tail)
        {
            if (_project.CurrentChart is null)
                return;

            if (tail.Time == head.Time)
                return;
            if (tail.Time <= head.Time)
                (head, tail) = (tail, head);

            var duration = tail.Time - head.Time;
            var rmv = _project.CurrentChart.GetRemoveNotesOperation(ImmutableArray.Create(tail));
            var edit = _project.CurrentChart.GetEditNotesDurationOperation(ImmutableArray.Create(head), duration);
            _context.Operations.Do(new CombinedOperation(rmv, edit));
        }

        public void InsertTempo(TempoRange range)
        {
            if (_project.CurrentProject is null)
                return;

            _context.Operations.Do(_project.CurrentProject
                .InsertTempo(range));
        }
    }
}
