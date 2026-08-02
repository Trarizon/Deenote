using Deenote.Models.Comparisons;
using System.Collections.Generic;

namespace Deenote.Contexts.EditorModels.Helpers
{
    internal sealed class NoteUniqueComparer : IComparer<INoteUnique>
    {
        public int Compare(INoteUnique x, INoteUnique y)
            => Comparer<uint>.Default.Compare(x.Uid, y.Uid);
    }

    internal sealed class NoteTimeUniqueComparer : IComparer<INoteUnique>
    {
        public int Compare(INoteUnique x, INoteUnique y)
        {
            var cmp = NoteComparers.ByTime.Compare(x, y);
            if (cmp != 0)
                return cmp;
            return Comparer<uint>.Default.Compare(x.Uid, y.Uid);
        }
    }
}
