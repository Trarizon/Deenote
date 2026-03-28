#nullable enable

using Deenote.Api.Operations;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Editing.Operations
{
    partial class ChartOperations
    {
        public static AddNoteOperation GetAddNoteOperation(this ChartEditorModel chart, NoteData note)
            => new AddNoteOperation(chart, note);

        public static AddNotesOperation GetAddNotesOperation(this ChartEditorModel chart, ReadOnlySpan<NoteData> notes)
            => new AddNotesOperation(chart, notes.ToImmutableArray());

        public static RemoveNotesOperation GetRemoveNotesOperation(this ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes)
            => new RemoveNotesOperation(chart, notes);

        [Obsolete("Builtin for chart concatenation, this may be changed or removed in the future")]
        public static ConcatNotesOperation GetConcatChartOperation(this ChartEditorModel chart, ChartData newChart, float offset, float multiplier) 
            => new ConcatNotesOperation(chart, newChart, offset, multiplier);

        public sealed class AddNoteOperation : NotifiableChartOperation<NoteEditorModel>
        {
            private readonly NoteEditorModel _model;

            internal AddNoteOperation(ChartEditorModel chart, NoteData note)
                : base(chart)
                => _model = new NoteEditorModel(note);

            protected override NoteEditorModel Redo()
            {
                Chart.AddNoteEditorModel(_model);
                return _model;
            }

            protected override NoteEditorModel Undo()
            {
                var model = _model;
                Chart.RemoveNoteEditorModel(model);
                return model;
            }
        }

        public sealed class AddNotesOperation : NotifiableChartOperation<ImmutableArray<NoteEditorModel>>
        {
            private readonly ImmutableArray<NoteData> _notes;

            private NoteEditorModel[]? _editorModels;

            internal AddNotesOperation(ChartEditorModel chart, ImmutableArray<NoteData> notes) : base(chart)
            {
                _notes = notes;
            }

            protected override ImmutableArray<NoteEditorModel> Redo()
            {
                if (_editorModels is null) {
                    _editorModels = new NoteEditorModel[_notes.Length];
                    for (int i = 0; i < _notes.Length; i++) {
                        _editorModels[i] = new NoteEditorModel(_notes[i]);
                    }
                }

                Chart.AddNoteEditorModels(_editorModels);
                return ImmutableCollectionsMarshal.AsImmutableArray(_editorModels);
            }

            protected override ImmutableArray<NoteEditorModel> Undo()
            {
                Debug.Assert(_editorModels is not null);
                var models = _editorModels!;
                Chart.RemoveNoteEditorModels(models);
                return ImmutableCollectionsMarshal.AsImmutableArray(models);
            }
        }

        public sealed class RemoveNotesOperation : NotifiableChartOperation<ImmutableArray<NoteEditorModel>>
        {
            private readonly ImmutableArray<NoteEditorModel> _notes;

            private IOperation _unlinkOperation;

            public RemoveNotesOperation(ChartEditorModel chart, ImmutableArray<NoteEditorModel> notes) : base(chart)
            {
                _notes = notes;
                _unlinkOperation = default!;
            }

            protected override ImmutableArray<NoteEditorModel> Redo()
            {
                _unlinkOperation.Redo();
                Chart.RemoveNoteEditorModels(_notes.AsSpan());
                return _notes;
            }

            protected override ImmutableArray<NoteEditorModel> Undo()
            {
                _unlinkOperation.Undo();
                Chart.AddNoteEditorModels(_notes.AsSpan());
                return _notes;
            }
        }

        [Obsolete("Builtin for chart concatenation, this may be changed or removed in the future")]
        public sealed class ConcatNotesOperation : NotifiableChartOperation<ChartEditorModel>
        {
            private readonly float _offset;
            private readonly float _multiplier;

            private readonly NoteEditorModel[] _newNotes;
            private readonly BackgroundNoteEditorModel[] _newBackgrounds;
            private readonly WarningNoteEditorModel[] _newWarnings;

            public ConcatNotesOperation(ChartEditorModel chart, ChartData newChart, float offset, float multiplier)
                : base(chart)
            {
                _offset = offset;
                _multiplier = multiplier;

                var newChartModel = new ChartModel(newChart);
                _newNotes = new NoteEditorModel[newChartModel.Notes.Count];
                for (int i = 0; i < newChartModel.Notes.Count; i++) {
                    var model = new NoteEditorModel(newChartModel.Notes[i]);
                    model.Time = _offset + (model.Time / _multiplier);
                    model.Duration /= _multiplier;
                    _newNotes[i] = model;
                }

                _newBackgrounds = new BackgroundNoteEditorModel[newChartModel.BackgroundNotes.Count];
                for (int i = 0; i < newChartModel.BackgroundNotes.Count; i++) {
                    var model = new BackgroundNoteEditorModel(newChartModel.BackgroundNotes[i]);
                    model.Time = _offset + (model.Time / _multiplier);
                    _newBackgrounds[i] = model;
                }

                _newWarnings = new WarningNoteEditorModel[newChartModel.WarningNotes.Count];
                for (int i = 0; i < newChartModel.WarningNotes.Count; i++) {
                    var model = new WarningNoteEditorModel(newChartModel.WarningNotes[i]);
                    model.Time = _offset + (model.Time / _multiplier);
                    _newWarnings[i] = model;
                }
            }

            protected override ChartEditorModel Redo()
            {
                Chart.AddNoteEditorModels(_newNotes);
                Chart.AddBackgroundNoteEditorModels(_newBackgrounds);
                Chart.AddWarningNoteEditorModels(_newWarnings);
                return Chart;
            }

            protected override ChartEditorModel Undo()
            {
                Chart.RemoveNoteEditorModels(_newNotes);
                Chart.RemoveBackgroundNoteEditorModels(_newBackgrounds);
                Chart.RemoveWarningNoteEditorModels(_newWarnings);
                return Chart;
            }
        }
    }
}
