using System;

namespace Deenote.Models
{
    public enum NoteKind
    {
        Click,
        Slide,
        Swipe,
    }

    [Obsolete("For json serialization only")]
    internal enum NoteType_Legacy { Hit = 0, Slide = 1 }
}