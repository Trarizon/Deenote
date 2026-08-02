using System;
using System.Diagnostics;

namespace Deenote.Models
{
    [DebuggerDisplay("Pos:{Position}, Time:{Time}")]
    public struct NoteCoord
    {
        public float Position;
        public float Time;

        public NoteCoord(float position, float time)
        {
            Position = position;
            Time = time;
        }

        #region Clamp

        public const float StageMaxPosition = 2f;
        public const float StageMaxPositionWidth = 2 * StageMaxPosition;

        public static NoteCoord ClampPosition(NoteCoord coord) => ClampPosition(coord.Position, coord.Time);

        public static NoteCoord ClampTime(NoteCoord coord, float maxTime)
            => coord with { Time = ChartConstraints.ClampTime(coord.Time, maxTime) };

        public static NoteCoord ClampPosition(float position, float time)
            => new(ChartConstraints.ClampPosition(position), time);

        public static NoteCoord Clamp(NoteCoord coord, float maxTime)
            => new(ChartConstraints.ClampPosition(coord.Position), ChartConstraints.ClampTime(coord.Time, maxTime));

        #endregion

        public static NoteCoord operator -(NoteCoord left, NoteCoord right) =>
            new(left.Position - right.Position, left.Time - right.Time);

        public static NoteCoord operator +(NoteCoord left, NoteCoord right) =>
            new(left.Position + right.Position, left.Time + right.Time);

        public readonly bool Equals(NoteCoord other) => Position == other.Position && Time == other.Time;
        public static bool operator ==(NoteCoord left, NoteCoord right) => left.Equals(right);
        public static bool operator !=(NoteCoord left, NoteCoord right) => !left.Equals(right);
        public override readonly string ToString() => $"{{ Pos:{Position}, Time:{Time} }}";
        public override readonly bool Equals(object? obj) => obj is NoteCoord coord && Equals(coord);
        public override readonly int GetHashCode() => HashCode.Combine(Position, Time);
    }
}