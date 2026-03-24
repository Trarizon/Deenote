#nullable enable

using Deenote.CoreB.IO.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
        }
    }
}
