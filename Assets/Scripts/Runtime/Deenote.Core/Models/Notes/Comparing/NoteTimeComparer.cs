#nullable enable

using Deenote.CoreB.Models.Charts;
using System.Collections.Generic;

namespace Deenote.CoreB.Models.Notes.Comparing
{
    public sealed class NoteTimeComparer : IComparer<INoteTime>, IComparer<SpeedLineData>
    {
        internal static NoteTimeComparer Instance { get; } = new NoteTimeComparer();
    
        public int Compare(INoteTime x, INoteTime y)
            => Comparer<float>.Default.Compare(x.Time, y.Time);

        public int Compare(SpeedLineData x, SpeedLineData y) 
            => Comparer<float>.Default.Compare(x.StartTime, y.StartTime);
    }
}
