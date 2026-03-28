#nullable enable

using Deenote.CoreB.Models.Notes.Comparers;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels.Comparing
{
    internal sealed class NoteTimeUniqueComaparer : IComparer<INoteTimeUnique>
    {
        public int Compare(INoteTimeUnique x, INoteTimeUnique y)
        {
            var cmp = NoteComparers.ViaTime.Compare(x, y);
            if (cmp != 0)
                return cmp;
            return Comparer<uint>.Default.Compare(x.Uid, y.Uid);
        }

        public bool Greater(INoteTimeUnique x, INoteTimeUnique y)
            => Compare(x, y) > 0;

        public bool Less(INoteTimeUnique x, INoteTimeUnique y)
            => Compare(x, y) < 0;
    }
}
