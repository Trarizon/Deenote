#nullable enable

using Deenote.Contexts;
using Deenote.Core.GamePlay;
using Deenote.CoreB.Models;
using Deenote.Editing.EditorModels;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.GameStage.Stage;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Editing.NoteSelection
{
    internal enum StageDragSelectionMode
    {
        Reset,
        Toggle,
    }

    internal sealed class StageDragSelector
    {
        private readonly ProjectContext _project;
        private readonly GameStageContext _stageContext;
        private readonly NoteSelectionContext _context;
        private readonly GamePlayContext _gamePlay;
        private readonly InputInterpreter _inputInterpreter;

        private NoteCoord _startCoord;
        private NoteCoord _endCoord;
        private DraggingState _state;
        private readonly List<NoteEditorModel> _inDragRangeNotes = new();

        private bool _enabled;

        [MemberNotNullWhen(true, nameof(GameStage))]
        public bool Enabled
        {
            get => _enabled && _stageContext.GameStage != null;
            set => _enabled = value;
        }

        private GameStageController? GameStage => _stageContext.GameStage;

        private StageDragSelectionMode _isToggleMode_bf;
        public StageDragSelectionMode SelectionMode
        {
            get => _isToggleMode_bf;
            set {
                if (Utils.SetField(ref _isToggleMode_bf, value)) {
                }
            }
        }

        internal StageDragSelector(NoteSelectionContext selection, GameStageContext stageContext, GamePlayContext gamePlay, ProjectContext project, InputInterpreter inputInterpreter)
        {
            _context = selection;
            _stageContext = stageContext;
            _gamePlay = gamePlay;
            _project = project;
            _inputInterpreter = inputInterpreter;
        }

        internal void OnStart()
        {
            _inputInterpreter.InputActions.NoteEdit.SelectAllNotes.started += (_) =>
            {
                if (_project.CurrentChart is not null) {
                    _context.ReselectNotes(_project.CurrentChart.Notes.AsSpan());
                }
            };
        }

        public event Action<StageDragSelector, SelectionAreaChangedEventArgs>? SelectionAreaChanged;

        public void BeginDragSelect(NoteCoord startCoord)
        {
            if (!Enabled)
                return;

            _startCoord = startCoord;
            _endCoord = startCoord;
            SelectionAreaChanged?.Invoke(this, new(_startCoord, _endCoord));
            _state = DraggingState.Started;

            if (SelectionMode is not StageDragSelectionMode.Toggle) {
                _context.ClearSelection();
            }
        }

        public void DraggingSelect(NoteCoord endCoord)
        {
            if (!Enabled)
                return;

            if (_state is DraggingState.Idle) {
                Debug.LogWarning("Unexpected selection state: Idle");
                return;
            }
            _state = DraggingState.Dragging;
            _endCoord = endCoord;
            UpdateDragSelection();
        }

        public void EndDragSelect(Vector2? viewportPoint)
        {
            if (!Enabled)
                return;

            if (_state is DraggingState.Idle) {
                Debug.LogWarning("Unexpected selection state: Idle");
                return;
            }
            _state = DraggingState.Idle;
            if (_startCoord == _endCoord) {
                if (viewportPoint is { } vp && GameStage.TryRaycastPerspectiveViewportPointToNote(vp, out var noteController)) {
                    var note = noteController.NoteModel;
                    _context.ReselectNote(note);
                }
            }
            _inDragRangeNotes.Clear();

            _startCoord = _endCoord = default;
            SelectionAreaChanged?.Invoke(this, new(_startCoord, _endCoord));
        }

        private void UpdateDragSelection()
        {
            var (start, end) = (_startCoord, _endCoord);

            NumberUtils.SortAsc(ref start.Position, ref end.Position);
            NumberUtils.SortAsc(ref start.Time, ref end.Time);
            SelectionAreaChanged?.Invoke(this, new(start, end));

            // TODO:OPTIMIZE:现在是全遍历
            // TODO: should consider note sprite size
            // 我在考虑从raycast映射后的coord直接就是考虑sprite size后的coord,
            // 这个size问题能不能试着在raycast2coord的方法里解决

            using var po_add = ListPool<NoteEditorModel>.Get(out var notesSelect);
            using var po_rmv = ListPool<NoteEditorModel>.Get(out var notesRemove);

            foreach (var note in _context.ProjectContext.CurrentChart.Notes) {
                bool inRange = !_stageContext.IsNoteDownplayed(note) && IsInSelectionRange(note, start, end);

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

            _context.ReplaceNotes(notesRemove.AsSpan(), notesSelect.AsSpan());
            notesRemove.Clear();
            notesSelect.Clear();

            bool IsInSelectionRange(NoteEditorModel note, NoteCoord start, NoteCoord end)
            {
                float pos = note.Position;
                float halfSize = note.Size / 2f;
                float time = note.Time;
                float speed = note.Speed;
                float currentTime = _gamePlay.CurrentTime;

                if (time < currentTime) {
                    return time >= start.Time
                        && time <= end.Time
                        && pos + halfSize >= start.Position
                        && pos - halfSize <= end.Position;
                }

                // TODO: Currently, select by drag down, and play stage backward, may make some note in EarlyDisplay mode have strange selection state
                // Theoretically I should judge if _game.EarlyDisplaySlowNotes, but its weird selecting in TimeOrder mode,
                // still considering how to implement t

                // The note is on stage
                if (time < currentTime + GameStage!.EvaluateNoteAppearAheadTime(speed)) {
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
                    => currentTime + (time - currentTime) * _stageContext.GetDisplaySpeed(speed);

                float ToAboveStagePseudoTime(float time, float speed)
                    => time + (GameStage!.NoteAppearAheadTime - GameStage.EvaluateNoteAppearAheadTime(speed));
            }
        }

        private enum DraggingState
        {
            Idle,
            Started,
            Dragging,
        }

        public readonly struct SelectionAreaChangedEventArgs
        {
            public NoteCoord Start { get; }
            public NoteCoord End { get; }
            public SelectionAreaChangedEventArgs(NoteCoord start, NoteCoord end)
            {
                Start = start;
                End = end;
            }
        }
    }
}
