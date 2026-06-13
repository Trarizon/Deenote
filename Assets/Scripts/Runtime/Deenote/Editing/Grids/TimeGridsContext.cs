#nullable enable

using Deenote.Contexts;
using Deenote.Core;
using Deenote.CoreB;
using Deenote.CoreB.Models;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels.Helpers;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using UnityEngine;

namespace Deenote.Editing.Grids
{
    public sealed class TimeGridsContext : INotifyPropertyChanged<TimeGridsContext>
    {
        private readonly ProjectContext _projectContext;

        private const int MinSubdivision = 1;
        private const int TimeGridMaxSubBeatCount = 64;
        /// <summary>
        /// If distance from time to grid &lt;= this, treat as equal
        /// </summary>
        private const float TimeGridMinEqualityThreshold = 1e-4f;

        private int _subdivisionPerBeat_bf;
        public int SubdivisionPerBeat
        {
            get => _subdivisionPerBeat_bf;
            set {
                value = Mathf.Clamp(value, MinSubdivision, TimeGridMaxSubBeatCount);
                if (Utils.SetField(ref _subdivisionPerBeat_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(SubdivisionPerBeat)));
                }
            }
        }

        public event Action<TimeGridsContext, PropertyEventArgs>? PropertyChanged;

        public TimeGridsContext(ProjectContext project, SaveSystem storage)
        {
            _projectContext = project;

            storage.SavingConfigurations += configs =>
            {
                configs.Set("stage/grids/time_grid_count", SubdivisionPerBeat);
            };

            storage.LoadedConfigurations += configs =>
            {
                SubdivisionPerBeat = configs.GetInt32("stage/grids/time_grid_count", 1);
            };
        }

        private ReadOnlySpan<Tempo> GetTempos()
        {
            var project = _projectContext.CurrentProject;
            if (project is null)
                return ReadOnlySpan<Tempo>.Empty;
            return project.Tempos.AsSpan();
        }

        /// <returns><see langword="null"/> if given time is earlier than first tempo or later than last beat line</returns>
        /// <remarks>
        /// If <paramref name="time"/> has same distance to neighboring lines, return the earlier one
        /// </remarks>
        public GridHitResult GetNearestGrid(float time)
        {
            if (SubdivisionPerBeat < MinSubdivision) {
                return GridHitResult.NoGrid();
            }

            var tempos = GetTempos();
            if (tempos.IsEmpty) {
                return GridHitResult.NoGrid();
            }

            var tempoIndex = tempos.GetTempoIndex(time);
            if (tempoIndex < 0) {
                return GridHitResult.ZeroBpm(tempos.Length > 0 ? tempos[0].StartTime : 0f);
            }

            var tempo = tempos[tempoIndex];
            if (tempo.Bpm == 0f) {
                // If last tempo and bpm is 0, the nearest line is the last tempo line
                if (tempoIndex == tempos.Length - 1) {
                    return GridHitResult.ZeroBpm(tempos[^1].StartTime);
                }
                // else, the neighboring lines are 2 tempo lines, find the nearest one
                else {
                    var nextTempo = tempos[tempoIndex + 1];
                    return GridHitResult.ZeroBpm(GetNear(time, tempo.StartTime, nextTempo.StartTime));
                }
            }

            int beatIndex = tempo.GetBeatIndex(time);
            float prevBeatTime = tempo.GetBeatTime(beatIndex);

            float prevBeatDelta = time - prevBeatTime;
            int subdivisionIndex = Mathf.FloorToInt(prevBeatDelta * SubdivisionPerBeat / tempo.BeatInterval);

            float prevSubdivisionTime = tempo.GetSubdivisionTime(beatIndex + (float)subdivisionIndex / SubdivisionPerBeat);
            float nextSubdivisionTime = tempo.GetSubdivisionTime(beatIndex + (float)(subdivisionIndex + 1) / SubdivisionPerBeat);

            // if tempo is not the last, subdivision line may not exists if near to next tempo, check it
            if (tempoIndex < tempos.Length - 1) {
                float nextTempoTime = tempos[tempoIndex + 1].StartTime;

                if (prevSubdivisionTime > nextTempoTime - (Tempo.MinBeatLineInterval / SubdivisionPerBeat)) {
                    // If a subdivision is near next tempo, the line is not exists, we should fallback to previous subdivision
                    // subdivisionIndex wont be 0 here, as the beat line (0 subdivision line) wont near to next tempo
                    Debug.Assert(subdivisionIndex > 0);
                    subdivisionIndex--;
                    prevSubdivisionTime = tempo.GetSubdivisionTime(beatIndex + (float)subdivisionIndex / SubdivisionPerBeat);
                }

                if (nextSubdivisionTime > nextTempoTime - (Tempo.MinBeatLineInterval / SubdivisionPerBeat)) {
                    // If a subdivision is near next tempo, the line is not exists, we should ceil to next tempo line
                    nextSubdivisionTime = nextTempoTime;
                }
            }

            return GridHitResult.Normal(GetNear(time, prevSubdivisionTime, nextSubdivisionTime));

            static float GetNear(float time, float prev, float next)
            {
                Debug.Assert(prev <= time);
                Debug.Assert(next >= time);
                float prevDelta = time - prev;
                float nextDelta = next - time;
                return prevDelta <= nextDelta ? prev : next;
            }
        }

        /// <returns><see langword="null"/> if given time is earlier than first tempo</returns>
        /// <remarks>
        /// Get the nearest grid before given time, if time is on grid, return the previous grid
        /// </remarks>
        public GridHitResult FloorToNearestNextGrid(float time)
        {
            if (SubdivisionPerBeat < MinSubdivision) {
                return GridHitResult.NoGrid();
            }

            var tempos = GetTempos();
            if (tempos.IsEmpty) {
                return GridHitResult.NoGrid();
            }

            var tempoIndex = tempos.GetCeilingTempoIndex(time) - 1;
            if (tempoIndex < 0) {
                return GridHitResult.ZeroBpm(0);
            }

            var tempo = tempos[tempoIndex];
            if (time - tempo.StartTime <= TimeGridMinEqualityThreshold) {
                return FloorToNearestNextGrid(tempo.StartTime);
            }

            if (tempo.Bpm == 0f) {
                return GridHitResult.ZeroBpm(tempo.StartTime);
            }

            int prevBeatIndex = tempo.GetCeilingBeatIndex(time) - 1;
            float prevBeatTime = tempo.GetBeatTime(prevBeatIndex);

            // Floating-point error handling
            if (Mathf.Approximately(prevBeatTime, time)) {
                prevBeatIndex--;
                prevBeatTime = tempo.GetBeatTime(prevBeatIndex);
            }

            var prevBeatDelta = time - prevBeatTime;
            int prevSubdivisionIndex = Mathf.CeilToInt(prevBeatDelta * SubdivisionPerBeat / tempo.BeatInterval) - 1;
            float prevSubdivisionTime = tempo.GetSubdivisionTime(prevBeatIndex + (float)prevSubdivisionIndex / SubdivisionPerBeat);

            // Floating-point error handling
            if (Mathf.Approximately(prevSubdivisionTime, time)) {
                prevSubdivisionIndex--;
                prevSubdivisionTime = tempo.GetSubdivisionTime(prevBeatIndex + (float)prevSubdivisionIndex / SubdivisionPerBeat);
            }

            if (time - prevSubdivisionTime <= TimeGridMinEqualityThreshold)
                return FloorToNearestNextGrid(prevSubdivisionTime);

            // if tempo is not the last, subdivision line may not exists if near to next tempo, check it
            if (tempoIndex < tempos.Length - 1) {
                var nextTempoTime = tempos[tempoIndex + 1].StartTime;
                if (prevSubdivisionTime > nextTempoTime - Tempo.MinBeatLineInterval / SubdivisionPerBeat) {
                    // If a subdivision is near next tempo, the line is not exists, we should fallback to previous subdivision
                    // subdivisionIndex wont be 0 here, as the beat line (0 subdivision line) wont near to next tempo
                    Debug.Assert(prevSubdivisionIndex > 0);
                    prevSubdivisionIndex--;
                    prevSubdivisionTime = tempo.GetSubdivisionTime(prevBeatIndex + (float)prevSubdivisionIndex / SubdivisionPerBeat);
                }
            }

            return GridHitResult.Normal(prevSubdivisionTime);
        }

        /// <returns><see langword="null"/> if given time is later than last line</returns>
        /// <remarks>
        /// Get the nearest grid after given time, if time is on grid, return the next grid
        /// </remarks>
        public GridHitResult CeilToNearestNextGrid(float time)
        {
            if (SubdivisionPerBeat < MinSubdivision) {
                return GridHitResult.NoGrid();
            }

            var tempos = GetTempos();
            if (tempos.IsEmpty) {
                return GridHitResult.NoGrid();
            }

            var tempoIndex = tempos.GetTempoIndex(time);
            if (tempoIndex < 0) {
                return GridHitResult.ZeroBpm(tempos[0].StartTime);
            }

            var tempo = tempos[tempoIndex];
            if (tempo.Bpm == 0f) {
                if (tempoIndex < tempos.Length - 1) {
                    return GridHitResult.ZeroBpm(tempos[tempoIndex + 1].StartTime);
                }
                return GridHitResult.PositiveInfinity();
            }

            if (tempoIndex < tempos.Length - 1) {
                float nextTempoTime = tempos[tempoIndex + 1].StartTime;
                // time is really near next tempo, we should ignore the next tempo time and ceil to the first subbeatline of next tempo
                if (time >= nextTempoTime - TimeGridMinEqualityThreshold)
                    return CeilToNearestNextGrid(nextTempoTime);
            }

            int prevBeatIndex = tempo.GetBeatIndex(time);
            float nextBeatTime = tempo.GetBeatTime(prevBeatIndex + 1);

            // Floating-point error handling
            if (Mathf.Approximately(nextBeatTime, time)) {
                prevBeatIndex++;
                nextBeatTime = tempo.GetBeatTime(prevBeatIndex + 1);
            }

            // time is really near
            if (time >= nextBeatTime - TimeGridMinEqualityThreshold)
                return CeilToNearestNextGrid(nextBeatTime);

            float prevBeatTime = tempo.GetBeatTime(prevBeatIndex);
            float prevBeatDelta = time - prevBeatTime;
            int nextSubdivisionIndex = Mathf.FloorToInt(prevBeatDelta * SubdivisionPerBeat / tempo.BeatInterval) + 1;
            float nextSubdivisionTime = tempo.GetSubdivisionTime(prevBeatIndex + (float)nextSubdivisionIndex / SubdivisionPerBeat);

            // Floating-point error handling
            if (Mathf.Approximately(nextSubdivisionTime, time)) {
                nextSubdivisionIndex++;
                nextSubdivisionTime = tempo.GetSubdivisionTime(prevBeatIndex + (float)nextSubdivisionIndex / SubdivisionPerBeat);
            }

            if (time >= nextSubdivisionTime - TimeGridMinEqualityThreshold)
                return CeilToNearestNextGrid(nextSubdivisionTime);

            if (tempoIndex < tempos.Length - 1) {
                var nextTempoTime = tempos[tempoIndex + 1].StartTime;
                if (nextSubdivisionTime > nextTempoTime - Tempo.MinBeatLineInterval / SubdivisionPerBeat) {
                    // `time` is between an unrendered beatTime and nextTempoTime,
                    // so the next nearest grid is next Tempo
                    nextSubdivisionTime = nextTempoTime;
                }
            }
            return GridHitResult.Normal(nextSubdivisionTime);
        }

        public GridsEnumerator EnumerateGrids(float startTime, float endTime)
        {
            var tempos = GetTempos();
            if (tempos.IsEmpty)
                return default;

            var tempoIndex = tempos.GetTempoIndex(startTime);
            if (tempoIndex < 0) {
                return new(tempos, (0, 0, 0), endTime, SubdivisionPerBeat);
            }

            var tempo = tempos[tempoIndex];
            if (tempo.Bpm == 0f) {
                return new(tempos, (tempoIndex + 1, 0, 0), endTime, SubdivisionPerBeat);
            }
            else {
                var beatIndex = tempo.GetBeatIndex(startTime);
                var beatTime = tempo.GetBeatTime(beatIndex);
                var beatTimeDelta = startTime - beatTime;
                var subdivisionInterval = tempo.BeatInterval / SubdivisionPerBeat;
                var subdivisionIndex = Mathf.Clamp(Mathf.FloorToInt(beatTimeDelta / subdivisionInterval), 0, SubdivisionPerBeat - 1);

                // Here we get the last grid before startTime, then we translate it to next line

                subdivisionIndex++;
                if (subdivisionIndex >= SubdivisionPerBeat) {
                    subdivisionIndex = 0;
                    beatIndex++;
                }
                var newBeatTime = tempo.GetBeatTime(beatIndex);
                if (newBeatTime >= tempos.GetSafeTempo(tempoIndex + 1).StartTime) {
                    beatIndex = 0;
                    tempoIndex++;
                }

                return new(tempos, (tempoIndex, beatIndex, subdivisionIndex), endTime, SubdivisionPerBeat);
            }
        }

        public ref struct GridsEnumerator
        {
            private readonly ReadOnlySpan<Tempo> _tempos;
            private readonly float _endTime;
            private readonly int _subdivisionPerBeat;

            public GridsEnumerator(ReadOnlySpan<Tempo> tempos, (int Tempo, int Beat, int Subdivision) startIndices, float endTime, int subdivisionPerBeat)
            {
                _tempos = tempos;
                _endTime = endTime;
                _subdivisionPerBeat = subdivisionPerBeat;
                _tempoIndex = startIndices.Tempo;
                _beatIndex = startIndices.Beat;
                _subdivisionIndex = startIndices.Subdivision;
            }

            public readonly GridsEnumerator GetEnumerator() => new(_tempos, (_tempoIndex, _beatIndex, _subdivisionIndex), _endTime, _subdivisionPerBeat);

            private int _tempoIndex;
            private int _beatIndex;
            private int _subdivisionIndex;

            private float _currentTime = default;
            private GridKind _currentKind = default;
            public readonly GridLineData Current => new(_currentTime, _currentKind);

            public bool MoveNext()
            {
            HandlingTempo:

                if (_tempoIndex >= _tempos.Length)
                    return false;

                var tempo = _tempos[_tempoIndex];
                int nextTempoIndex = _tempoIndex + 1;
                float nextTempoTime = nextTempoIndex < _tempos.Length ? _tempos[nextTempoIndex].StartTime : float.PositiveInfinity;

                // Draw tempo
                if (_beatIndex == 0 && _subdivisionIndex == 0) {
                    if (tempo.StartTime > _endTime) {
                        _tempoIndex = _tempos.Length; // force end
                        return false;
                    }
                    _currentTime = tempo.StartTime;
                    _currentKind = GridKind.Tempo;
                    if (tempo.Bpm == 0) {
                        // When bpm is 0, no beat subdivision should be rendered
                        _tempoIndex++;
                    }
                    else {
                        _subdivisionIndex++;
                    }
                    return true;
                }

            HandlingBeat:

                float beatTime = tempo.GetBeatTime(_beatIndex);

                // Draw a tempo or beat
                if (_subdivisionIndex == 0 && _subdivisionPerBeat > 0) {
                    // To close to next Tempo line, ignore this
                    if (beatTime >= nextTempoTime - Tempo.MinBeatLineInterval) {
                        _tempoIndex++;
                        _beatIndex = 0;
                        _subdivisionIndex = 0;
                        goto HandlingTempo;
                    }
                    if (beatTime >= _endTime) {
                        _tempoIndex = _tempos.Length; // force end
                        return false;
                    }
                    _currentTime = beatTime;
                    _currentKind = GridKind.Beat;
                    _subdivisionIndex++;
                    return true;
                }

                // Handle subdivision

                if (_subdivisionIndex >= _subdivisionPerBeat) {
                    _beatIndex++;
                    _subdivisionIndex = 0;
                    goto HandlingBeat;
                }

                var subdivisionTime = tempo.GetSubdivisionTime(_beatIndex + (float)_subdivisionIndex / _subdivisionPerBeat);

                // Current subdivision is later than next Tempo line, should render next tempo
                var minSubdivisionInterval = Tempo.MinBeatLineInterval / _subdivisionPerBeat;
                if (subdivisionTime >= nextTempoTime - minSubdivisionInterval) {
                    _tempoIndex++;
                    _beatIndex = 0;
                    _subdivisionIndex = 0;
                    goto HandlingTempo;
                }

                if (subdivisionTime >= _endTime) {
                    _tempoIndex = _tempos.Length; // force end
                    return false;
                }

                _currentTime = subdivisionTime;
                _currentKind = GridKind.Subdivision;
                _subdivisionIndex++;
                return true;
            }
        }

        public enum GridKind { Tempo, Beat, Subdivision }

        public readonly record struct GridLineData(
            float Time,
            GridKind Kind);

        public enum GridStatus
        {
            /// <summary>
            /// No grid in current settings
            /// </summary>
            NoGrid,
            /// <summary>
            /// Represents a grid at positive infinity
            /// </summary>
            Infinity,
            /// <summary>
            /// The note is in range where bpm is 0
            /// </summary>
            ZeroBpm,
            Normal,
        }

        public readonly struct GridHitResult
        {
            public readonly float Time;
            public readonly GridStatus Status;

            public float? Value => Status is GridStatus.Normal ? Time : null;

            public bool HasValue => Status is GridStatus.Normal or GridStatus.ZeroBpm;

            private GridHitResult(float time, GridStatus status)
            {
                Time = time;
                Status = status;
            }

            public static GridHitResult NoGrid() => new GridHitResult(float.NaN, GridStatus.NoGrid);
            public static GridHitResult ZeroBpm(float time) => new(time, GridStatus.ZeroBpm);
            public static GridHitResult PositiveInfinity() => new(float.PositiveInfinity, GridStatus.Infinity);
            public static GridHitResult Normal(float time) => new(time, GridStatus.Normal);
        }
    }
}
