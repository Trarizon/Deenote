#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Core;
using Deenote.CoreB.Models;
using Deenote.CoreB.Notification;
using Deenote.Library;
using System;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace Deenote.Editing.Grids
{
    public sealed class PositionGridsContext : INotifyPropertyChanged<PositionGridsContext>
    {
        private const int MinGridCount = 2;
        public const int PositionGridMaxCount = 41;
        /// <summary>
        /// If distance from position to grid &lt;= this, treat as equal
        /// </summary>
        private const float PositionGridMinEqualityThreshold = 1e-4f;

        private int _positionGridCount_bf;
        public int GridCount
        {
            get => _positionGridCount_bf;
            set {
                value = Mathf.Clamp(value, MinGridCount, PositionGridMaxCount);
                if (Utils.SetField(ref _positionGridCount_bf, value)) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(GridCount)));
                }
            }
        }

        public event Action<PositionGridsContext, PropertyEventArgs>? PropertyChanged;

        public PositionGridsContext(SaveSystem storage)
        {
            storage.SavingConfigurations += configs =>
            {
                configs.Set("stage/grids/pos_grid_count", GridCount);
            };
            storage.LoadedConfigurations += configs =>
            {
                GridCount = configs.GetInt32("stage/grids/pos_grid_count", 9);
            };
        }

        public float GetGridPosition(int index)
        {
            var grid = GetLinePosition(index, GridCount, out var inRange);
            if (!inRange)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
            return grid;
        }

        public float GetNearestGrid(float position)
        {
            // Guard
            if (GridCount < MinGridCount) {
                Debug.LogAssertion("PositionGridCount < MinGridCount");
                return position;
            }

            var normalized = (position - NoteConstraints.StageMinPosition) / NoteConstraints.StageMaxPositionWidth;
            normalized = Mathf.Clamp01(normalized);
            int idx = Mathf.RoundToInt(normalized * (GridCount - 1));
            return GetLinePosition(idx, GridCount, out _);
        }

        /// <summary>
        /// get the nearest grid on left, if current position is already on grid, return the left one,
        /// if no left grids, return null
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float? FloorToNearestNextGrid(float position)
        {
            if (GridCount < MinGridCount) {
                Debug.LogAssertion("PositionGridCount < MinGridCount");
                return null;
            }

            for (int i = GridCount - 1; i >= 0; i--) {
                float gridPos = GetLinePosition(i, GridCount, out _);
                if (gridPos + PositionGridMinEqualityThreshold < position)
                    return gridPos;

            }
            Debug.Assert(position <= NoteConstraints.StageMinPosition + PositionGridMinEqualityThreshold);
            return position == NoteConstraints.StageMinPosition ? null : NoteConstraints.StageMinPosition;
        }

        /// <summary>
        /// get the nearest grid on right, if current position is already on grid, return the right one,
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float? CeilToNearestNextGrid(float position)
        {
            if (GridCount < MinGridCount) {
                Debug.LogAssertion("PositionGridCount < MinGridCount");
                return null;
            }

            for (int i = 0; i < GridCount; i++) {
                float gridPos = GetLinePosition(i, GridCount, out _);
                if (gridPos - PositionGridMinEqualityThreshold > position)
                    return gridPos;
            }
            Debug.Assert(position >= NoteConstraints.StageMaxPosition - PositionGridMinEqualityThreshold);
            return position == NoteConstraints.StageMaxPosition ? null : NoteConstraints.StageMaxPosition;
        }

        [Pure]
        private static float GetLinePosition(int index, int count, out bool inRange)
        {
            Debug.Assert(count >= MinGridCount);
            inRange = (uint)index < (uint)count;
            return NoteConstraints.StageMinPosition + NoteConstraints.StageMaxPositionWidth * ((float)index / (count - 1));
        }
    }
}
