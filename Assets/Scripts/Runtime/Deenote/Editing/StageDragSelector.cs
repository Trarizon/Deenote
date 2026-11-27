using Deenote.Core;
using Deenote.Core.GameStage;
using Deenote.Core.Helpers;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Project;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Editing
{
    internal enum DragSelectionMode { Reselect, Toggle, }

    internal sealed class StageDragSelector
    {
        private readonly ProjectManager2 _projectManager;
        private readonly GameStageManager _stageManager;
        private readonly GamePlayManager2 _gamePlayManager;
        private readonly ChartNotesSelector _selector;

        public StageDragSelector(ChartNotesSelector selector, ProjectManager2 projectManager, GameStageManager stageManager, GamePlayManager2 gamePlayManager)
        {
            _selector = selector;
            _projectManager = projectManager;
            _stageManager = stageManager;
            _gamePlayManager = gamePlayManager;

            _gamePlayManager.TimeChanged += args =>
            {
                if (!IsDragSelecting)
                    return;

                var delta = args.NewTime - args.OldTime;
                _endCoord.Time += delta;
                UpdateDragSelection();
            };
        }

        private DragSelectionMode _mode_bf;
        public DragSelectionMode Mode
        {
            get => _mode_bf;
            set => _mode_bf = value;
        }

        private bool _enabled_bf = true;
        public bool Enabled
        {
            get => _enabled_bf;
            set => _enabled_bf = value;
        }

        private DraggingState _state;
        private NoteCoord _startCoord;
        private NoteCoord _endCoord;
        private readonly HashSet<NoteModel> _inDragRangeNotes = new();
        private readonly HashSet<SpeedChangeWarningModel> _inDragRangeWarnings = new();

        public bool IsDragSelecting => _state is DraggingState.Dragging;

        public event Action<StageDragSelector, SelectionAreaChangedEventArgs>? SelectionAreaChanged;

        public void BeginDragSelect(NoteCoord coord)
        {
            if (!Enabled) {
                App.Logger.LogWarning("Drag selector is disabled, cannot begin drag");
                return;
            }

            _startCoord = coord;
            _endCoord = coord;

            _state = DraggingState.Started;

            if (Mode is DragSelectionMode.Reselect) {
                _selector.ClearSelection();
                RefreshAllOnStage();
            }

            SelectionAreaChanged?.Invoke(this, new SelectionAreaChangedEventArgs(_startCoord, _endCoord));
        }

        public void DraggingSelect(NoteCoord coord)
        {
            if (!Enabled) {
                App.Logger.LogWarning("Drag selector is disabled, cannot drag");
                return;
            }

            if (_state is DraggingState.Idle) {
                App.Logger.LogWarning("Unexpected drag state: Idle, cannot drag");
                return;
            }

            _state = DraggingState.Dragging;
            _endCoord = coord;

            UpdateDragSelection();
        }

        public void EndDragSelect(Vector2? viewportPoint)
        {
            if (!Enabled) {
                App.Logger.LogWarning("Drag selector is disabled, cannot end drag");
                return;
            }

            if (_state is DraggingState.Idle) {
                App.Logger.LogWarning("Unexpected drag state: Idle, cannot end drag");
                return;
            }

            _state = DraggingState.Idle;
            if (_startCoord == _endCoord) {
                if (viewportPoint is { } vp) {
                    if (_stageManager.TryRaycastPerspectiveViewportPointToNote(vp, out var note)) {
                        _selector.Reselect(note.NoteModel);
                        RefreshAllOnStage();
                    }
                    else if (_stageManager.TryRaycastPerspectiveViewportPointToWarningNote(vp, out var warning)) {
                        _selector.Reselect(warning.Model);
                        RefreshAllOnStage();
                    }
                }
                return;
            }

            _inDragRangeNotes.Clear();
            _inDragRangeWarnings.Clear();
            _startCoord = _endCoord = default;
            SelectionAreaChanged?.Invoke(this, new SelectionAreaChangedEventArgs(_startCoord, _endCoord));
        }

        private void UpdateDragSelection()
        {
            Asserts.NotNull(_projectManager.CurrentChart);

            var (start, end) = (_startCoord, _endCoord);

            NumberUtils.SortAsc(ref start.Position, ref end.Position);
            NumberUtils.SortAsc(ref start.Time, ref end.Time);

            SelectionAreaChanged?.Invoke(this, new SelectionAreaChangedEventArgs(start, end));

            // Optimize:现在是全遍历
            // TODO: should consider note sprite size
            // 我在考虑从raycast映射后的coord直接就是考虑sprite size后的coord，
            // 这个size问题能不能试着在raycast2coord的方法里解决

            using var pn_add = ListPool<NoteModel>.Get(out var notesSelect);
            using var pn_rmv = ListPool<NoteModel>.Get(out var notesRemove);

            foreach (var note in _projectManager.CurrentChart.EnumerateNoteModels()) {
                bool inRange = _stageManager.IsNoteHighlighted(note) && IsNoteInSelectionRange(note);

                if (_inDragRangeNotes.Contains(note)) {
                    if (!inRange) {
                        _inDragRangeNotes.Remove(note);
                        if (note.IsSelected)
                            notesRemove.Add(note);
                        else
                            notesSelect.Add(note);
                    }
                }
                else {
                    if (inRange) {
                        _inDragRangeNotes.Add(note);
                        if (note.IsSelected)
                            notesRemove.Add(note);
                        else
                            notesSelect.Add(note);
                    }
                }
            }

            // TODO: arch1.1: maybe i should use custom type to replace the ObservableList<T> ...
            _selector.SelectedNotes.RemoveAll(notesRemove, static (ns, n) => ns.Contains(n));
            _selector.SelectedNotes.AddRange(notesSelect);
            _selector.SelectedNotes.Sort(NodeTimeUniqueComparer.Instance);
            notesRemove.Clear();
            notesSelect.Clear();

            using var pw_add = ListPool<SpeedChangeWarningModel>.Get(out var warningsSelect);
            using var pw_rmv = ListPool<SpeedChangeWarningModel>.Get(out var warningsRemove);

            foreach (var note in _projectManager.CurrentChart.SpeedChangeWarnings) {
                bool inRange = _stageManager.WarningNoteVisibility.IsShowNote() && IsWarningInSelectionRange(note);

                if (_inDragRangeWarnings.Contains(note)) {
                    if (!inRange) {
                        _inDragRangeWarnings.Remove(note);
                        if (note.IsSelected)
                            warningsRemove.Add(note);
                        else
                            warningsSelect.Add(note);
                    }
                }
                else {
                    if (inRange) {
                        _inDragRangeWarnings.Add(note);
                        if (note.IsSelected)
                            warningsRemove.Add(note);
                        else
                            warningsSelect.Add(note);
                    }
                }
            }

            _selector.SelectedWarningNotes.RemoveAll(warningsRemove, static (ws, w) => ws.Contains(w));
            _selector.SelectedWarningNotes.AddRange(warningsSelect);
            _selector.SelectedWarningNotes.Sort(NodeTimeComparer.Instance);
            warningsRemove.Clear();
            warningsSelect.Clear();

            RefreshAllOnStage();

            bool IsNoteInSelectionRange(NoteModel note)
                => IsInSelectionRange(note.Position, note.Size, note.Time, note.Speed);

            bool IsWarningInSelectionRange(SpeedChangeWarningModel warning)
                => IsInSelectionRange(GameStageSpeedChangeWarningNoteController.SpritePosition, 1f, warning.Time, _stageManager.PlacementNoteSpeed);

            bool IsInSelectionRange(float pos, float size, float time, float speed)
            {
                float halfSize = size / 2f;
                float currentTime = _gamePlayManager.CurrentTime;

                if (time < currentTime) {
                    return time >= start.Time
                        && time <= end.Time
                        && pos + halfSize >= start.Position
                        && pos - halfSize <= end.Position;
                }

                // TODO: Currently, select by drag down, and play stage backward, may make some note in EarlyDisplay mode have strange selection state
                // Theoretically I should judge if _game.EarlyDisplaySlowNotes, but its weird selecting in TimeOrder mode,
                // still considering how to implement it

                // The note is on stage
                if (time < currentTime + App.GameStageManager.GetStageNoteAppearAheadTime(speed, true)) {
                    var pseudoTime = ToPseudoTime(time);
                    return pseudoTime >= start.Time
                        && pseudoTime <= end.Time
                        && pos + halfSize >= start.Position
                        && pos - halfSize <= end.Position;
                }
                // The note is not on stage
                else {
                    var pseudoTime = ToAboveStagePseudoTime(time, speed);
                    return pseudoTime >= start.Time
                        && pseudoTime <= end.Time
                        && pos + halfSize >= start.Position
                        && pos - halfSize <= end.Position;
                }

                float ToPseudoTime(float time)
                    => currentTime + (time - currentTime) * _stageManager.ApplySpeedDiff(speed);

                float ToAboveStagePseudoTime(float time, float speed)
                    => time + (_stageManager.StandardNoteAppearAheadTime - _stageManager.GetStageNoteAppearAheadTime(speed, true));
            }
        }

        private void RefreshAllOnStage()
        {
            foreach (var note in _stageManager.ActiveNotes) {
                note.RefreshColoring();
            }
            foreach (var note in _stageManager.ActiveWarningNotes) {
                note.RefreshColoring();
            }
        }

        private enum DraggingState { Idle, Started, Dragging, }

        public readonly struct SelectionAreaChangedEventArgs
        {
            // StartCoord < EndCoord
            public NoteCoord StartCoord { get; }
            public NoteCoord EndCoord { get; }

            internal SelectionAreaChangedEventArgs(NoteCoord startCoord, NoteCoord endCoord)
            {
                StartCoord = startCoord;
                EndCoord = endCoord;
            }
        }
    }
}