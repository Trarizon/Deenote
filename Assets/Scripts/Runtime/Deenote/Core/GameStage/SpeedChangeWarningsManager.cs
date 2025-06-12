#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.Collections.Generic;
using Deenote.Library.Mathematics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Core.GameStage
{
    public sealed partial class SpeedChangeWarningsManager
    {
        private readonly GamePlayManager _game;
        private ObjectPool<GameStageSpeedChangeWarningNoteController> _pool = default!;
        private readonly SortedList<GameStageSpeedChangeWarningNoteController> _trackingNotes;

        // See comments on NotesManager
        private int _nextHitNoteIndex;
        private int _nextActiveNoteIndex;

        internal SpeedChangeWarningsManager(GamePlayManager manager)
        {
            _game = manager;
            _trackingNotes = new(Comparer<GameStageSpeedChangeWarningNoteController>.Create((l, r) =>
            {
                return NodeTimeComparer.Instance.Compare(l.Model, r.Model);
            }));
        }

        /// <summary>
        /// The model that should shows on ui, <see langword="null"/> is no warning now
        /// </summary>
        public SpeedChangeWarningModel? CurrentWarningModel
        {
            get {
                _game.AssertChartLoaded();

                if (_nextHitNoteIndex == 0)
                    return default;

                var models = _game.CurrentChart.SpeedChangeWarnings.AsSpan();
                var current = _nextHitNoteIndex - 1;
                var model = models[current];
                var currentTime = _game.MusicPlayer.Time;

                Debug.Assert(model.Time < currentTime);
                if (currentTime - model.Time > GamePlayManager.SpeedWarningDuration) {
                    return null;
                }
                return model;
            }
        }

        internal void Initialzie(ObjectPool<GameStageSpeedChangeWarningNoteController> speedWarningNotePool)
        {
            if (_pool is not null) {
                ClearTrackingNotes();
            }

            _pool = speedWarningNotePool;
            _nextHitNoteIndex = 0;
            _nextActiveNoteIndex = 0;
        }

        private void ReselectActiveVisibleNotes()
        {
            _game.AssertChartLoaded();
            _game.AssertChartLoaded();

            ClearTrackingNotes();

            var chart = _game.CurrentChart;
            var models = chart.SpeedChangeWarnings.AsSpan();

            var currentTime = _game.MusicPlayer.Time;
            var activateDeltaTime = _game.GetStageNoteActiveAheadTime(_game.HighlightedNoteSpeed);
            var activateNoteTime = currentTime + activateDeltaTime;

            int index = 0;

            for (; index < models.Length; index++) {
                var node = models[index];
                if (node.Time > currentTime)
                    break;
            }
            _nextHitNoteIndex = index;

            for (; index < models.Length; index++) {
                var model = models[index];
                if (model.Time > activateNoteTime)
                    break;
                AddTrackingModel(model);
            }
            _nextActiveNoteIndex = index;

            foreach (var item in _trackingNotes) {
                item.RefreshStageDeltaTime();
            }
        }

        internal void RefreshActiveModels()
        {
            ReselectActiveVisibleNotes();
        }

        internal void ShiftActiveModels()
        {
            // TODO: Optimize
            ReselectActiveVisibleNotes();
        }

        #region Collection

        private void ClearTrackingNotes()
        {
            foreach (var item in _trackingNotes) {
                _pool.Release(item);
            }
            _trackingNotes.Clear();
        }

        private void AddTrackingModel(SpeedChangeWarningModel model)
        {
            var item = _pool.Get();
            item.Initialize(model);

            Debug.Assert(!_trackingNotes.Contains(item));
            _trackingNotes.Add(item);
        }

        #endregion
    }
}