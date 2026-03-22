#nullable enable

using Deenote.Entities.Models;
using System.Collections.Generic;

namespace Deenote.Entities.Comparisons
{
    public static class NoteComparers
    {
        public static NodeTimeComparer NodeTime => NodeTimeComparer.Instance;

        public static void AssertInTimeOrder(IEnumerable<IStageNoteNode> notes, string? additionMessage = null)
            => NodeTimeComparer.AssertInOrder(notes, additionMessage);
    }
}
