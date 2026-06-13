#nullable enable

using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Contexts;
using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Editing.Grids;
using Deenote.Editing.NotePlacement;
using Deenote.Editing.NoteSelection;
using Deenote.Library.Collections;
using Deenote.ProjectManagement;
using System;

namespace Deenote.Editing
{
    public sealed class EditorManager
    {
        private readonly ProjectContext _project;
        private readonly EditorContext _context;
        private readonly ProjectManagerB _projectManager;
        private readonly ChartNotesEditor _chartEditor;
        private readonly MouseEditingCoordinator _mouseEditingCoordinator;
        private readonly InputInterpreter _inputInterpreter;

        internal EditorManager(ProjectContext project, EditorContext context, ProjectManagerB projectManager, ChartNotesEditor chartEditor, MouseEditingCoordinator mouseEditingCoordinator, InputInterpreter inputInterpreter)
        {
            _project = project;
            _context = context;
            _projectManager = projectManager;
            _chartEditor = chartEditor;
            _mouseEditingCoordinator = mouseEditingCoordinator;
            _inputInterpreter = inputInterpreter;
        }

        internal void OnStart()
        {
            _project.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentProject))) {
                    _context.Operations.Reset();
                }
            });

            _projectManager.ProjectSaved += () =>
            {
                _context.Operations.MarkSaveAtCurrent();
            };

            RegisterInputs();
        }

        private void RegisterInputs()
        {
            var actions = _inputInterpreter.InputActions.NoteEdit;
            actions.SelectAllNotes.started += (_) =>
            {
                if (_project.CurrentChart is not null) {
                    _context.NoteSelection.SelectNotes(_project.CurrentChart.Notes.AsSpan());
                }
            };
            actions.RemoveSelectedNotes.started += _ => _chartEditor.RemoveSelectedNotes();
            actions.Copy.started += _ => AddNotesToClipBoard(_context.NoteSelection.SelectedNotes);
            actions.Cut.started += _ =>
            {
                AddNotesToClipBoard(_context.NoteSelection.SelectedNotes);
                _chartEditor.RemoveSelectedNotes();
            };
            actions.Paste.started += _ => _mouseEditingCoordinator.TryPreparePaste();

            actions.Redo.started += _ => _context.Operations.Redo(null);
            actions.Undo.started += _ => _context.Operations.Undo(null);

            // Notes Edit

            const float TimeDelta = 0.001f;
            const float TimeDeltaLarge = 0.01f;
            const float PositionDelta = 0.01f;
            const float PositionDeltaLarge = 0.1f;
            const float SizeDelta = 0.01f;
            const float SizeDeltaLarge = 0.1f;
            const float SpeedDelta = 0.01f;
            const float SpeedDeltaLarge = 0.1f;
            const float DurationDelta = 0.001f;
            const float DurationDeltaLarge = 0.01f;

            actions.TimeDec.started += _ => _chartEditor.EditNotesTime(_context.NoteSelection.SelectedNotes, t => t - TimeDelta);
            actions.TimeInc.started += _ => _chartEditor.EditNotesTime(_context.NoteSelection.SelectedNotes, t => t + TimeDelta);
            actions.TimeDecLarge.started += _ => _chartEditor.EditNotesTime(_context.NoteSelection.SelectedNotes, t => t - TimeDeltaLarge);
            actions.TimeIncLarge.started += _ => _chartEditor.EditNotesTime(_context.NoteSelection.SelectedNotes, t => t + TimeDeltaLarge);
            actions.TimeDecByGrid.started += _ => _chartEditor.EditNotesTime(_context.NoteSelection.SelectedNotes, t => _context.Grids.TimeGrids.FloorToNearestNextGrid(t).Value ?? t);
            actions.TimeIncByGrid.started += _ => _chartEditor.EditNotesTime(_context.NoteSelection.SelectedNotes, t => _context.Grids.TimeGrids.CeilToNearestNextGrid(t).Value ?? t);
            actions.PositionLeft.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => p - PositionDelta);
            actions.PositionRight.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => p + PositionDelta);
            actions.PositionLeftLarge.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => p - PositionDeltaLarge);
            actions.PositionRightLarge.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => p + PositionDeltaLarge);
            actions.PositionLeftByGrid.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => _context.Grids.PositionGrids.FloorToNearestNextGrid(p) ?? p);
            actions.PositionRightByGrid.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => _context.Grids.PositionGrids.CeilToNearestNextGrid(p) ?? p);
            actions.PositionMirror.started += _ => _chartEditor.EditNotesPosition(_context.NoteSelection.SelectedNotes, p => -p);
            actions.CoordQuantize.started += _ => _chartEditor.EditNotesCoord(_context.NoteSelection.SelectedNotes, c => _context.Grids.Quantize(c, true, true));
            actions.SizeDec.started += _ => _chartEditor.EditNotesSize(_context.NoteSelection.SelectedNotes, s => s - SizeDelta);
            actions.SizeInc.started += _ => _chartEditor.EditNotesSize(_context.NoteSelection.SelectedNotes, s => s + SizeDelta);
            actions.SizeDecLarge.started += _ => _chartEditor.EditNotesSize(_context.NoteSelection.SelectedNotes, s => s - SizeDeltaLarge);
            actions.SizeIncLarge.started += _ => _chartEditor.EditNotesSize(_context.NoteSelection.SelectedNotes, s => s + SizeDeltaLarge);
            actions.SpeedDec.started += _ => _chartEditor.EditNotesSpeed(_context.NoteSelection.SelectedNotes, s => s - SpeedDelta);
            actions.SpeedInc.started += _ => _chartEditor.EditNotesSpeed(_context.NoteSelection.SelectedNotes, s => s + SpeedDelta);
            actions.SpeedDecLarge.started += _ => _chartEditor.EditNotesSpeed(_context.NoteSelection.SelectedNotes, s => s - SpeedDeltaLarge);
            actions.SpeedIncLarge.started += _ => _chartEditor.EditNotesSpeed(_context.NoteSelection.SelectedNotes, s => s + SpeedDeltaLarge);
            actions.KindClick.started += _ => _chartEditor.EditNotesKind(_context.NoteSelection.SelectedNotes, NoteKind.Click);
            actions.KindSlide.started += _ => _chartEditor.EditNotesKind(_context.NoteSelection.SelectedNotes, NoteKind.Slide);
            actions.KindSwipe.started += _ => _chartEditor.EditNotesKind(_context.NoteSelection.SelectedNotes, NoteKind.Swipe);
            actions.SoundAdd.started += _ => _chartEditor.EditNotesSounds(_context.NoteSelection.SelectedNotes, true);
            actions.SoundRemove.started += _ => _chartEditor.EditNotesSounds(_context.NoteSelection.SelectedNotes, false);
            actions.DurationDec.started += _ => _chartEditor.EditNotesDuration(_context.NoteSelection.SelectedNotes, d => d - DurationDelta);
            actions.DurationInc.started += _ => _chartEditor.EditNotesDuration(_context.NoteSelection.SelectedNotes, d => d + DurationDelta);
            actions.DurationDecLarge.started += _ => _chartEditor.EditNotesDuration(_context.NoteSelection.SelectedNotes, d => d - DurationDeltaLarge);
            actions.DurationIncLarge.started += _ => _chartEditor.EditNotesDuration(_context.NoteSelection.SelectedNotes, d => d + DurationDeltaLarge);
            actions.DurationDecByGrid.started += _ => _chartEditor.EditNotesEndTime(_context.NoteSelection.SelectedNotes, t => _context.Grids.TimeGrids.FloorToNearestNextGrid(t).Value ?? t);
            actions.DurationIncByGrid.started += _ => _chartEditor.EditNotesEndTime(_context.NoteSelection.SelectedNotes, t => _context.Grids.TimeGrids.CeilToNearestNextGrid(t).Value ?? t);
            actions.CreateHoldBetween.started += _ =>
            {
                var selectedNotes = _context.NoteSelection.SelectedNotes;
                if (selectedNotes.Length != 2)
                    return;

                var prev = selectedNotes[0];
                var next = selectedNotes[1];
                _chartEditor.CreateHoldBetween(prev, next);
            };
        }

        public void AddNotesToClipBoard(ReadOnlySpan<NoteEditorModel> notes)
        {
            if (notes.IsEmpty)
                return;

            using var so_notes = SpanOwner<NoteData>.Allocate(notes.Length);
            var noteDatas = so_notes.Span;
            for (int i = 0; i < notes.Length; i++) {
                noteDatas[i] = notes[i].ToDataNonLinkInfo();
            }
            NoteLinkHelpers.CloneLinkInfos(notes, noteDatas);
            _context.ClipBoard.SetNotes(noteDatas);
        }
    }
}
