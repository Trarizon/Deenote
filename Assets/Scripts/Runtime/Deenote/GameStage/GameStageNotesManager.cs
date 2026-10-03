using CommunityToolkit.Diagnostics;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.CoreB;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.GameStage.Models;
using Deenote.Project;
using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    /// <summary>
    /// Manages which notes are active on stage<br/>
    /// Data only, not related to scene display
    /// </summary>
    public sealed partial class GameStageNotesManager
    {
        private readonly GameStageManager _stage;
        private readonly GamePlayManager _gamePlay;
        private readonly ProjectManager2 _project;

        private readonly GameStageNodeActiveTimeComparer _comparer;

        private int _nextInactiveNoteIndex;
        private int _nextHitNoteIndex;
        private int _nextActiveNoteIndex;
        private int _nextActiveNoteIndexInActiveOrder;
        private int _currentCombo;

        public int CurrentCombo { get => _currentCombo; private set => _currentCombo = value; }

        private readonly List<NoteModel> _trackingNotes = new();
        private readonly List<NoteModel> _trackingNotesActiveOrder = new();

        public ReadOnlySpan<NoteModel> ActiveNotes => _trackingNotes.AsSpan();

        internal GameStageNotesManager(GameStageManager stage, GamePlayManager gamePlay, ProjectManager2 project)
        {
            _stage = stage;
            _gamePlay = gamePlay;
            _project = project;
            _comparer = new(_stage);
        }

        public event Action<GameStageNotesManager, ActiveNotesChangedEventArgs>? ActiveNotesChanged;

        #region Query

        /// <returns>
        /// ComboNode that just reached judge line,
        /// <see langword="null"/> if current combo is 0 or chart is null
        /// </returns>
        public IStageNoteNode? GetPreviousHitComboNode()
        {
            if (_project.CurrentChart is null) {
                App.Logger.LogWarning("Chart is null");
                return null;
            }

            for (int i = _nextHitNoteIndex - 1; i >= 0; i--) {
                var note = _project.CurrentChart.NoteNodes[i];
                if (note.IsComboNode)
                    return note;
            }
            return null;
        }

        public NoteModel? GetPreviousHitNote()
        {
            if (_project.CurrentChart is null) {
                App.Logger.LogWarning("Chart is null");
                return null;
            }

            for (int i = _nextHitNoteIndex - 1; i >= 0; i--) {
                var note = _project.CurrentChart.NoteNodes[i];
                if (note is NoteModel noteModel)
                    return noteModel;
            }
            return null;
        }

        public IStageNoteNode? GetNextActiveNodeInTimeOrderDisplayMode()
        {
            Guard.IsNotNull(_project.CurrentChart);

            var nodes = _project.CurrentChart.NoteNodes.AsSpan();
            if (_nextActiveNoteIndex >= nodes.Length)
                return null;

            return nodes[_nextActiveNoteIndex];
        }

        #endregion

        private void RefreshActiveVisibleNotes(float currentTime)
        {
            var chart = _project.CurrentChart;
            if (chart is null)
                return;

            ClearTrackNotes();

            var config = _stage.Config;
            var noteNodes = chart.NoteNodes.AsSpan();

            // OPTIMIZE:
            // 两种思路，一种是这里用list，每次刷新时排序，
            // 另一种是缓存这个List，但是需要实时同步谱面中note的增减以及顺序变化
            using var lp_noteNodesInActiveOrder = ListPool<IStageNoteNode>.Get(out var noteNodesInActiveOrderList);
            noteNodesInActiveOrderList.Replace(noteNodes);
            noteNodesInActiveOrderList.Sort(_comparer);
            var noteNodesInActiveOrder = noteNodesInActiveOrderList.AsSpan();

            var deactivateDeltaTime = config.NoteHitEffectMaxDuration;
            var deactivateNoteTime = currentTime - deactivateDeltaTime;
            var activateDeltaTime = _stage.StandardNoteActiveAheadTime;
            var activateNoteTime = currentTime + activateDeltaTime;

            int index = 0, combo = 0;

            // Iterate nods before current stage
            // Should track if note tail is on stage
            for (; index < noteNodes.Length; index++) {
                var note = noteNodes[index];
                if (note.Time > deactivateNoteTime)
                    break;
                TryIncrementCombo(note, ref combo);
                TryTrackNote(note, deactivateNoteTime);
            }
            _nextInactiveNoteIndex = index;

            // Iterate nodes in hit effect time
            for (; index < noteNodes.Length; index++) {
                var note = noteNodes[index];
                if (note.Time > currentTime)
                    break;
                TryIncrementCombo(note, ref combo);
                TryTrackNote(note, deactivateNoteTime);
            }
            _nextHitNoteIndex = index;
            _currentCombo = combo;

            // Iterate nodes in falling time to find _nextActiveNoteIndex;
            // Notes may have different speed, so we do not track notes here
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node is NoteModel && new GameStageNodeView(node, _stage).GetPseudoTime(currentTime) > activateNoteTime)
                    break;
            }
            _nextActiveNoteIndex = index;

            // Iterate nodes in falling time, by appear time order
            var indexInActiveOrder = noteNodesInActiveOrder
                .FindLowerBoundIndex(new GameStageNodeActiveTimeComparable(currentTime, _stage));
            for (int i = indexInActiveOrder - 1; i >= 0; i--) {
                var node = noteNodesInActiveOrder[i];
                if (node is NoteModel note) {
                    if (note.Time > currentTime) {
                        AddTrackNote(note);
                    }
                }
            }
            _nextActiveNoteIndexInActiveOrder = indexInActiveOrder;

            _trackingNotes.Sort(NodeTimeUniqueComparer.Instance);
            _trackingNotesActiveOrder.Sort(_comparer);

            ActiveNotesChanged?.Invoke(this, new(_trackingNotes));

            static void TryIncrementCombo(IStageNoteNode node, ref int combo)
            {
                if (node.IsComboNode)
                    combo++;
            }

            void TryTrackNote(IStageNoteNode node, float deactivateNoteTime)
            {
                if (node is not NoteModel note)
                    return;
                if (note.EndTime > deactivateNoteTime) {
                    AddTrackNote(note);
                }
            }
        }

        public void RefreshActiveNotes()
        {
            RefreshActiveVisibleNotes(_gamePlay.MusicPlayer.Time);
        }

        /// <summary>
        /// Called when music time changed
        /// </summary>
        public void ShiftActiveNotes(bool playSounds)
        {
            RefreshActiveNotes();
        }

        #region Collection

        private void AddTrackNote(NoteModel note)
        {
            _trackingNotes.Add(note);
            _trackingNotesActiveOrder.Add(note);
        }

        private void RemoveTrackNote(NoteModel note)
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
            private readonly List<NoteModel> _notes;
            public ReadOnlySpan<NoteModel> ActiveNotes => _notes.AsSpan();

            public ActiveNotesChangedEventArgs(List<NoteModel> activeNotes)
            {
                _notes = activeNotes;
            }
        }
    }
}