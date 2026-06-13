#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library.Collections;
using System;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace Deenote.Editing.Operations
{
    internal static class NoteOperations
    {
        /// <remarks>
        /// DO NOT use this method edit note Time / Position / Duration / Kind / Sounds
        /// </remarks>
        public static EditNotesPropertyOperation<T> GetEditNotesOperation<T>(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, string propertyName, T value,
            Func<NoteEditorModel, T> getter, Action<NoteEditorModel, T> setter)
            => new SimpleEditNotesPropertyOperation<T>(chart, propertyName, notes, getter, setter, value);

        /// <remarks>
        /// DO NOT use this method edit note Time / Position / Duration / Kind / Sounds
        /// </remarks>
        public static EditNotesPropertyOperation<T> GetEditNotesOperation<T>(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, string propertyName, Func<T, T> valueSelector,
            Func<NoteEditorModel, T> valueGetter, Action<NoteEditorModel, T> valueSetter)
            => new SimpleEditNotesPropertyOperation<T>(chart, propertyName, notes, valueGetter, valueSetter, valueSelector);

        // Time

        public static EditNotesPropertyOperation<float> GetEditNotesTimeOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, float value)
            => new EditNotesTimePropertyOperation(chart, notes, value);

        public static EditNotesPropertyOperation<float> GetEditNotesTimeOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, Func<float, float> valueSelector)
            => new EditNotesTimePropertyOperation(chart, notes, valueSelector);

        // Position

        public static EditNotesPropertyOperation<float> GetEditNotesPositionOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, float value)
            => new EditNotesPositionPropertyOperation(chart, notes, value);

        public static EditNotesPropertyOperation<float> GetEditNotesPositionOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, Func<float, float> valueSelector)
            => new EditNotesPositionPropertyOperation(chart, notes, valueSelector);

        // Coord

        public static EditNotesPropertyOperation<NoteCoord> GetEditNotesCoordOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, NoteCoord value)
            => new EditNotesCoordPropertyOperation(chart, notes, value);

        public static EditNotesPropertyOperation<NoteCoord> GetEditNotesCoordOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, Func<NoteCoord, NoteCoord> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes, valueSelector);

        // Duration

        public static EditNotesPropertyOperation<float> GetEditNotesDurationOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, float value)
            => new EditNotesDurationPropertyOperation(chart, notes, value);

        public static EditNotesPropertyOperation<float> GetEditNotesDurationOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, Func<float, float> valueSelector)
            => new EditNotesDurationPropertyOperation(chart, notes, valueSelector);

        // EndTime

        public static EditNotesPropertyOperation<float> GetEditNotesEndTimeOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, float value)
            => new EditNotesEndTimePropertyOperation(chart, notes, value);

        public static EditNotesPropertyOperation<float> GetEditNotesEndTimeOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, Func<float, float> valueSelector)
            => new EditNotesEndTimePropertyOperation(chart, notes, valueSelector);

        // NoteKind

        public static EditNotesPropertyOperation<NoteKind> GetEditNotesKindOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, NoteKind value)
            => new EditNotesKindPropertyOperation(chart, notes, value);

        public static EditNotesPropertyOperation<NoteKind> GetEditNotesKindOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, Func<NoteKind, NoteKind> valueSelector)
            => new EditNotesKindPropertyOperation(chart, notes, valueSelector);

        // Sounds

        public static EditNotesPropertyOperation<ImmutableArray<PianoSoundData>> GetEditNotesSoundsOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes, ImmutableArray<PianoSoundData> value)
            => new EditNotesSoundsProperyOperation(chart, notes, value);


        public abstract class EditNotesPropertyOperation<TProperty> : NotifiableChartOperation<ImmutableArray<NoteEditorModel>>
        {
            protected const int FirstRedoing = 0;
            protected const int Redoing = 1;
            protected const int Undoing = 2;

            protected ImmutableArray<NoteEditorModel> Notes { get; }
            protected ImmutableArray<TProperty> OldValues { get; }
            private readonly Func<TProperty, TProperty>? _selector;
            private readonly TProperty? _newValue;

            private TProperty[]? _newValues;

            internal EditNotesPropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                Func<NoteEditorModel, TProperty> valueGetter,
                ValueProvider<TProperty> valueProvider) : base(chart)
            {
                Notes = notes;
                OldValues = notes.Select(valueGetter).ToImmutableArray();
                _selector = valueProvider.Selector;
                _newValue = valueProvider.Value;
            }

            protected override ImmutableArray<NoteEditorModel> Redo()
            {
                bool firstTime = _newValues is null;
                int status = firstTime ? FirstRedoing : Redoing;
                OnRedoing(firstTime);

                if (_selector is not null) {
                    _newValues ??= new TProperty[Notes.Length];
                    for (int i = 0; i < Notes.Length; i++) {
                        _newValues[i] = _selector(OldValues[i]);
                    }

                    for (int i = 0; i < Notes.Length; i++) {
                        SetValue(status, i, _newValues[i]);
                    }
                }
                else {
                    Debug.Assert(_newValue is not null);
                    _newValues ??= Array.Empty<TProperty>();
                    for (int i = 0; i < Notes.Length; i++) {
                        SetValue(status, i, _newValue!);
                    }
                }

                OnRedone(firstTime);
                OnDone();
                return Notes;
            }

            protected abstract void SetValue(int status, int index, TProperty newValue);
            protected virtual void OnRedoing(bool firstTime) { }
            protected virtual void OnRedone(bool firstTime) { }

            protected override ImmutableArray<NoteEditorModel> Undo()
            {
                OnUndoing();
                if (_selector is not null) {
                    for (int i = Notes.Length - 1; i >= 0; i--) {
                        SetValue(Undoing, i, OldValues[i]);
                    }
                }
                else {
                    Debug.Assert(_newValue is not null);
                    for (int i = Notes.Length - 1; i >= 0; i--) {
                        SetValue(Undoing, i, OldValues[i]);
                    }
                }
                OnUndone();
                OnDone();
                return Notes;
            }

            protected virtual void OnUndoing() { }
            protected virtual void OnUndone() { }

            protected virtual void OnDone() { }
        }

        private sealed class SimpleEditNotesPropertyOperation<TProperty> : EditNotesPropertyOperation<TProperty>
        {
            private readonly string _propertyName;
            private readonly Action<NoteEditorModel, TProperty> _setter;
            internal SimpleEditNotesPropertyOperation(ChartEditorModel chart, string propertyName,
                ImmutableArray<NoteEditorModel> notes,
                Func<NoteEditorModel, TProperty> valueGetter,
                Action<NoteEditorModel, TProperty> valueSetter,
                ValueProvider<TProperty> valueProvider)
                : base(chart, notes, valueGetter, valueProvider)
            {
                this._propertyName = propertyName;
                _setter = valueSetter;
            }

            protected override void SetValue(int status, int index, TProperty newValue)
            {
                _setter(Notes[index], newValue);
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), _propertyName);
            }
        }

        public sealed class EditNotesTimePropertyOperation : EditNotesPropertyOperation<float>
        {
            internal EditNotesTimePropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ValueProvider<float> valueProvider) :
                base(chart, notes, x => x.Time, valueProvider)
            { }

            protected override void SetValue(int status, int index, float newValue)
            {
                var note = Notes[index];
                NoteCollisionHelpers.ReupdateCollisionPreMoving(Chart, note);
                note.Time = newValue;
                NoteLinkHelpers.ReorderLink(note);
                NoteCollisionHelpers.ReupdateCollisionPostMoving(Chart, note);
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.Time));
            }
        }

        public sealed class EditNotesPositionPropertyOperation : EditNotesPropertyOperation<float>
        {
            internal EditNotesPositionPropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ValueProvider<float> valueProvider) :
                base(chart, notes, x => x.Position, valueProvider)
            { }

            protected override void SetValue(int status, int index, float newValue)
            {
                var note = Notes[index];
                NoteCollisionHelpers.ReupdateCollisionPreMoving(Chart, note);
                note.Position = newValue;
                NoteCollisionHelpers.ReupdateCollisionPostMoving(Chart, note);
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.Position));
            }
        }

        public sealed class EditNotesCoordPropertyOperation : EditNotesPropertyOperation<NoteCoord>
        {
            internal EditNotesCoordPropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ValueProvider<NoteCoord> valueProvider) :
                base(chart, notes, x => x.PositionCoord, valueProvider)
            { }

            protected override void SetValue(int status, int index, NoteCoord newValue)
            {
                var note = Notes[index];
                NoteCollisionHelpers.ReupdateCollisionPreMoving(Chart, note);
                note.PositionCoord = newValue;
                NoteLinkHelpers.ReorderLink(note);
                NoteCollisionHelpers.ReupdateCollisionPostMoving(Chart, note);
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.PositionCoord));
            }
        }

        public abstract class EditNotesDurationPropertyOperation<T> : EditNotesPropertyOperation<T>
        {
            internal EditNotesDurationPropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                Func<NoteEditorModel, T> valueGetter,
                ValueProvider<T> valueProvider)
                : base(chart, notes, valueGetter, valueProvider)
            {
            }

            protected override void SetValue(int status, int index, T newValue)
            {
                var note = Notes[index];
                NoteDurationHelpers.SetDuration(Chart, note, GetDuration(note, newValue));
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.Duration));
            }

            protected abstract float GetDuration(NoteEditorModel note, T value);
        }

        private sealed class EditNotesDurationPropertyOperation : EditNotesDurationPropertyOperation<float>
        {
            internal EditNotesDurationPropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ValueProvider<float> valueProvider)
                : base(chart, notes, x => x.Duration, valueProvider)
            { }

            protected override float GetDuration(NoteEditorModel note, float value) => value;
        }

        private sealed class EditNotesEndTimePropertyOperation : EditNotesDurationPropertyOperation<float>
        {
            internal EditNotesEndTimePropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ValueProvider<float> valueProvider)
                : base(chart, notes, x => x.EndTime, valueProvider)
            { }

            // TODO: Move this clamp call to editor
            protected override float GetDuration(NoteEditorModel note, float value) => Mathf.Max(0f, value - note.Time);
        }

        public sealed class EditNotesKindPropertyOperation : EditNotesPropertyOperation<NoteKind>
        {
            private readonly LinkInfo[] _linkInfos;

            internal EditNotesKindPropertyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ValueProvider<NoteKind> valueProvider)
                : base(chart, notes, x => x.Kind, valueProvider)
            {
                _linkInfos = new LinkInfo[notes.Length];
                for (int i = 0; i < notes.Length; i++) {
                    var note = notes[i];
                    _linkInfos[i] = new LinkInfo(note.PrevLink, note.NextLink);
                }
            }

            protected override void SetValue(int status, int index, NoteKind newValue)
            {
                var note = Notes[index];

                if (note.Kind is NoteKind.Slide) {
                    NoteLinkHelpers.UnlinkRemainingChain(note);
                }

                note.Kind = newValue;

                if (newValue is NoteKind.Slide) {
                    if (index > 0) {
                        var prev = Notes[index - 1];
                        prev.NextLink = note;
                        note.PrevLink = prev;
                    }
                }
            }

            protected override void OnUndone()
            {
                for (int i = 0; i < Notes.Length; i++) {
                    var note = Notes[i];
                    var (prev, next) = _linkInfos[i];
                    if (note.IsSlide) {
                        note.PrevLink = prev;
                        if (prev is not null)
                            prev.NextLink = note;
                        note.NextLink = next;
                        if (next is not null)
                            next.PrevLink = note;
                    }
                    else {
                        // Maybe here is setting link to null, but I forgot why I've wrote such a complex segment
                        if (note.PrevLink != prev) {
                            note.PrevLink = prev;
                            if (prev is not null)
                                prev.NextLink = note;
                        }
                        if (note.NextLink != next) {
                            note.NextLink = next;
                            if (next is not null)
                                next.PrevLink = note;
                        }
                    }
                }
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.Kind));
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.PrevLink));
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.NextLink));
            }

            private readonly record struct LinkInfo(
                INoteLink? PrevLink,
                INoteLink? NextLink);
        }

        public sealed class EditNotesSoundsProperyOperation : EditNotesPropertyOperation<ImmutableArray<PianoSoundData>>
        {
            internal EditNotesSoundsProperyOperation(ChartEditorModel chart,
                ImmutableArray<NoteEditorModel> notes,
                ImmutableArray<PianoSoundData> newValue)
                : base(chart, notes, n => n.HasSounds ? n.Sounds.ToImmutableArray() : ImmutableArray<PianoSoundData>.Empty, newValue)
            { }

            protected override void SetValue(int status, int index, ImmutableArray<PianoSoundData> newValue)
            {
                Notes[index].Sounds.Replace(newValue.AsSpan());
            }

            protected override void OnDone()
            {
                Chart.RaiseNoteEditorModelsPropertyChanged(Notes.AsSpan(), nameof(NoteEditorModel.Sounds));
            }
        }

        internal readonly struct ValueProvider<T>
        {
            public readonly T? Value;
            public readonly Func<T, T>? Selector;

            public ValueProvider(T value)
            {
                Value = value;
                Selector = null;
            }

            public ValueProvider(Func<T, T> selector)
            {
                Value = default;
                Selector = selector;
            }

            public static implicit operator ValueProvider<T>(T value) => new ValueProvider<T>(value);
            public static implicit operator ValueProvider<T>(Func<T, T> selector) => new ValueProvider<T>(selector);
        }
    }
}
