#nullable enable

using Deenote.CoreB.Models.Charts;
using System;
using System.Collections.Generic;

namespace Deenote.CoreB.Models.Notes.Comparers
{
    public sealed class NoteTimeComparer : IComparer<INoteTime>, IComparer<SpeedLineData>
    {
        internal static NoteTimeComparer Instance { get; } = new NoteTimeComparer();
    
        public int Compare(INoteTime x, INoteTime y)
            => Comparer<float>.Default.Compare(x.Time, y.Time);

        public int Compare(SpeedLineData x, SpeedLineData y) 
            => Comparer<float>.Default.Compare(x.StartTime, y.StartTime);
    }

    public readonly struct NoteTimeComparable : IComparable<INoteTime>
    {
        private readonly float _value;

        public NoteTimeComparable(float time) => _value = time;

        public int CompareTo(INoteTime other) => _value.CompareTo(other.Time);
    }
}
