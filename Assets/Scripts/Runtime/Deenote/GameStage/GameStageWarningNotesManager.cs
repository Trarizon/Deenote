#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.CoreB;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using Deenote.GameStage.Models;
using Deenote.Library.Collections.Generic;
using Deenote.Project;
using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    /// <summary>
    /// Maintains the warning notes that are currently on stage. Warnings are not combo nodes
    /// and have no hit effect, so their time window is intentionally independent from
    /// <see cref="NotesManager"/>.
    /// </summary>
    public sealed class GameStageWarningNotesManager
    {
        private readonly GameStageManager _stage;
        private readonly GamePlayManager2 _gamePlay;
        private readonly ProjectManager2 _project;
        private readonly List<SpeedChangeWarningModel> _trackingWarnings = new();

        internal GameStageWarningNotesManager(GameStageManager stage, GamePlayManager2 gamePlay, ProjectManager2 project)
        {
            _stage = stage;
            _gamePlay = gamePlay;
            _project = project;
        }

        public event Action<GameStageWarningNotesManager, ActiveNotesChangedEventArgs>? ActiveNotesChanged;

        internal ReadOnlySpan<SpeedChangeWarningModel> ActiveNotes => _trackingWarnings.AsSpan();

        private void RefreshActiveVisibleNotes(float currentTime)
        {
            var chart = _project.CurrentChart;
            if (chart is null)
                return;

            _trackingWarnings.Clear();

            var activateTime = currentTime + _stage.GetNoteActiveAheadTime(_stage.PlacementNoteSpeed);
            var warnings = chart.SpeedChangeWarnings;

            var idx = warnings.AsSpan().FindLowerBoundIndex(new WarningNoteTimeComparable(currentTime));

            foreach (var warning in warnings.AsSpan(idx..)) {
                if (warning.Time > activateTime)
                    break;

                _trackingWarnings.Add(warning);
            }

            ActiveNotesChanged?.Invoke(this, new(_trackingWarnings));
        }

        public void RefreshActiveNotes()
        {
            RefreshActiveVisibleNotes(_gamePlay.CurrentTime);
        }

        internal void ShiftActiveNotes()
        {
            RefreshActiveNotes();
        }

        public readonly struct ActiveNotesChangedEventArgs
        {
            private readonly List<SpeedChangeWarningModel> _notes;
            public ReadOnlySpan<SpeedChangeWarningModel> ActiveNotes => _notes.AsSpan();

            internal ActiveNotesChangedEventArgs(List<SpeedChangeWarningModel> activeNotes)
            {
                _notes = activeNotes;
            }
        }
    }
}
