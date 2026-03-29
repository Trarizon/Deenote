#nullable enable

using Deenote.Contexts;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.CoreB.Models;
using Deenote.Editing.EditorModels;
using Deenote.Library.Collections;
using Deenote.Library.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Core.Editing
{
    internal enum StageDragSelectionMode
    {
        Reset,
        Toggle,
    }

    internal sealed class StageDragSelector
    {
        private readonly NoteSelectionContext _context;
        private readonly GamePlayManager _gamePlay;

        private NoteCoord _startCoord;
        private NoteCoord _endCoord;
        private DraggingState _state;
        private readonly List<NoteEditorModel> _inDragRangeNotes = new();

        private bool _enabled;

        [MemberNotNullWhen(true, nameof(GameStage))]
        public bool Enabled
        {
            get => _enabled && _gamePlay.Stage != null;
            set => _enabled = value;
        }

        private GameStageController? GameStage => _gamePlay.Stage;

        public StageDragSelector(NoteSelectionContext context, GamePlayManager gamePlay)
        {
            _context = context;
            _gamePlay = gamePlay;
        }

        public event Action<StageDragSelector, SelectionAreaChangedEventArgs>? SelectionAreaChanged;

        public void BeginDragSelect(NoteCoord startCoord, StageDragSelectionMode mode)
        {
            if (!Enabled)
                return;

            _startCoord = startCoord;
            _endCoord = startCoord;
            SelectionAreaChanged?.Invoke(this, new(_startCoord, _endCoord));
            _state = DraggingState.Started;

            if (mode is not StageDragSelectionMode.Toggle) {
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
                bool inRange = !_gamePlay.IsNoteDownplayed(note) && IsInSelectionRange(note, start, end);

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
                float currentTime = _gamePlay.MusicPlayer.Time;

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
                    => currentTime + (time - currentTime) * _gamePlay.GetDisplayNoteSpeed(speed);

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
