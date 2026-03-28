#nullable enable

namespace Deenote.CoreB.Models.Notes
{
    partial class NoteData
    {
        public static class Marshal
        {
            public static void Link(NoteData prev, NoteData next)
            {
                prev._nextLink = next;
                next._prevLink = prev;
            }

            public static void UnlinkNext(NoteData note)
            {
                if (note._nextLink is not null)
                    note._nextLink._prevLink = null;
                note._nextLink = null;
            }

            public static void UnlinkPrev(NoteData note)
            {
                if (note._prevLink is not null)
                    note._prevLink._nextLink = null;
                note._prevLink = null;
            }
        }
    }
}
