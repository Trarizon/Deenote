#nullable enable

using System.Collections.Generic;

namespace Deenote.Editing.EditorModels.Comparing
{
    internal sealed class NoteUniqueComparer : IComparer<INoteUnique>
    {
        public int Compare(INoteUnique x, INoteUnique y)
            => Comparer<uint>.Default.Compare(x.Uid, y.Uid);

        public bool Greater(INoteUnique x, INoteUnique y)
            => Compare(x, y) > 0;

        public bool Less(INoteUnique x, INoteUnique y)
            => Compare(x, y) < 0;
    }
}
