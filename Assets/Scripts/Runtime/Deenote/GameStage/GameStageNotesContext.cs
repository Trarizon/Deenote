#nullable enable

using Deenote.Contexts;
using Deenote.CoreB;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.GamePlay;
using Deenote.GameStage.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    internal sealed class GameStageNotesContext : INotifyPropertyChanged<GameStageNotesContext>
    {
        private readonly GameStageContext _stage;
        private readonly ProjectContext _project;

        public GameStageContext StageContext => _stage;

        private int _nextInactiveNoteIndex;
        private int _nextHitNoteIndex;
        private int _nextActiveNoteIndex;
        private int _nextActiveNoteIndexInActiveOrder;
        private int _currentCombo;

        public int CurrentCombo
        {
            get => _currentCombo;
            private set {
                if (Utils.SetField(ref _currentCombo, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CurrentCombo)));
                }
            }
        }

        private float _currentTime_bf;
        /// <summary>
        /// This is not proxy of <see cref="GamePlayContext.CurrentTime"/>, but will be synced to that
        /// </summary>
        public float CurrentTime
        {
            get => _currentTime_bf;
            private set {
                if (_currentTime_bf != value) {
                    _currentTime_bf = value;
                    RefreshActiveVisibleNotes(value);
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CurrentTime)));
                }
            }
        }

        private readonly List<NoteEditorModel> _trackingNotes;
        private readonly List<NoteEditorModel> _trackingNotesActiveOrder;

        public event Action<GameStageNotesContext, ActiveNotesChangedEventArgs>? ActiveNotesChanged;
        public event Action<GameStageNotesContext, PropertyEventArgs>? PropertyChanged;

        public event Action<GameStageNotesContext>? Updated;

        private readonly GameStageNodeActiveTimeComparer _nodeActiveTimeComparer;

        public ReadOnlySpan<NoteEditorModel> ActiveNotes => _trackingNotes.AsSpan();

        public GameStageNotesContext(GameStageContext stage, ProjectContext project, GamePlayContext gamePlay)
        {
            _stage = stage;
            _project = project;
            _trackingNotes = new List<NoteEditorModel>();
            _trackingNotesActiveOrder = new List<NoteEditorModel>();
            _nodeActiveTimeComparer = new GameStageNodeActiveTimeComparer(stage);

            _stage.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.IsApplySpeedDifference)) ||
                    e.MatchProperty(nameof(s.ActualNoteFallSpeed))) {
                    RefreshActiveVisibleNotes();
                }
            });

            gamePlay.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentTime))) {
                    CurrentTime = s.CurrentTime;
                }
            });

            _project.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    RefreshActiveVisibleNotes();
                }
            });
        }

        /// <returns>
        /// ComboNode that just reached judge line,
        /// <see langword="null"/> if current combo is 0 or chart is null
        /// </returns>
        public IGameStageNoteNode? GetPreviousHitComboNode()
        {
            if (_project.CurrentChart is null)
                return null;
            for (int i = _nextHitNoteIndex - 1; i >= 0; i--) {
                var note = _project.CurrentChart.NoteNodes[i];
                if (note.IsComboNode)
                    return note;
            }
            return null;
        }

        public IGameStageNoteNode? GetNextActiveNodeInTimeOrderDisplayMode()
        {
            var nodes = _project.CurrentChart.NoteNodes.AsSpan();
            if (_nextActiveNoteIndex >= nodes.Length)
                return null;
            else
                return nodes[_nextActiveNoteIndex];
        }

        internal void RefreshActiveVisibleNotes()
        {
            RefreshActiveVisibleNotes(CurrentTime);
        }

        private void RefreshActiveVisibleNotes(float currentTime)
        {
            var chart = _project.CurrentChart;
            if (chart is null)
                return;
            if (_stage.ThemeContext.CurrentTheme is null)
                return;

            ClearTrackNotes();

            var config = _stage.ThemeContext.CurrentTheme.Config;
            var noteNodes = chart.NoteNodes.AsSpan();

            // OPTIMIZE:
            // 两种思路，一种是这里用list，每次刷新时排序，
            // 另一种是缓存这个List，但是需要实时同步谱面中note的增减以及顺序变化
            using var lp_noteNodesInActiveOrder = ListPool<IGameStageNoteNode>.Get(out var noteNodesInActiveOrderList);
            noteNodesInActiveOrderList.Replace(noteNodes);
            noteNodesInActiveOrderList.Sort(_nodeActiveTimeComparer);
            var noteNodesInActiveOrder = noteNodesInActiveOrderList.AsSpan();

            var deactiveDeltaTime = config.NoteHitEffectMaxDuration;
            var deactiveNoteTime = currentTime - deactiveDeltaTime;
            var activeDeltaTime = config.GetNoteActiveAheadTime(_stage.ActualNoteFallSpeed);
            var activeNoteTime = currentTime + activeDeltaTime;

            int index = 0, combo = 0;

            // Iterate nods before current stage
            // Should track if note tail is on stage
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node.Time > deactiveNoteTime)
                    break;
                TryIncrementCombo(node, ref combo);
                TryTrackNote(node);
            }
            _nextInactiveNoteIndex = index;

            // Iterate nodes in hit effect time
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node.Time > currentTime)
                    break;
                TryIncrementCombo(node, ref combo);
                TryTrackNote(node);
            }
            _nextHitNoteIndex = index;
            CurrentCombo = combo;

            // Iterate nodes in falling time to find _nextActiveNoteIndex;
            // Notes may have different speed, so we do not track notes here
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node is NoteEditorModel && new GameStageNodeView(node, _stage).GetPseudoTime(currentTime) > activeNoteTime)
                    break;
            }
            _nextActiveNoteIndex = index;

            // Iterate nodes in falling time, by appear time order
            var indexInActiveOrder = noteNodesInActiveOrder
                .FindLowerBoundIndex(new GameStageNodeActiveTimeComparable(currentTime, _stage));
            for (int i = indexInActiveOrder - 1; i >= 0; i--) {
                var node = noteNodesInActiveOrder[i];
                if (node is NoteEditorModel note) {
                    if (note.Time > currentTime) {
                        AddTrackNote(note);
                    }
                }
            }
            _nextActiveNoteIndexInActiveOrder = indexInActiveOrder;

            _trackingNotes.Sort(ModelComparers.ViaTimeUnique);
            _trackingNotesActiveOrder.Sort(_nodeActiveTimeComparer);

            // Debug.Log($"Refresh notes: {_trackingNotes.Count}");
            ActiveNotesChanged?.Invoke(this, new(_trackingNotes));
            Updated?.Invoke(this);

            static void TryIncrementCombo(IGameStageNoteNode node, ref int combo)
            {
                if (node.IsComboNode)
                    combo++;
            }

            void TryTrackNote(IGameStageNoteNode node)
            {
                if (node is not NoteEditorModel note)
                    return;
                if (note.EndTime > deactiveNoteTime) {
                    AddTrackNote(note);
                }
            }
        }

        #region Collection

        private void AddTrackNote(NoteEditorModel note)
        {
            _trackingNotes.Add(note);
            _trackingNotesActiveOrder.Add(note);
        }

        private void RemoveTrackNote(NoteEditorModel note)
        {
            _trackingNotes.Remove(note);
            _trackingNotesActiveOrder.Remove(note);
        }

        private void ClearTrackNotes()
        {
            _trackingNotes.Clear();
            _trackingNotesActiveOrder.Clear();
        }

        #endregion

        public readonly struct ActiveNotesChangedEventArgs
        {
            private readonly List<NoteEditorModel> _notes;
            public ReadOnlySpan<NoteEditorModel> ActiveNotes => _notes.AsSpan();

            internal ActiveNotesChangedEventArgs(List<NoteEditorModel> activeNotes)
            {
                _notes = activeNotes;
            }
        }

        public readonly ref struct ActiveNotesChangeEventArgs
        {
            public ReadOnlySpan<NoteEditorModel> Added { get; }
            public ReadOnlySpan<NoteEditorModel> Removed { get; }

            public ActiveNotesChangeEventArgs(ReadOnlySpan<NoteEditorModel> added, ReadOnlySpan<NoteEditorModel> removed)
            {
                Added = added;
                Removed = removed;
            }
        }
    }
}
