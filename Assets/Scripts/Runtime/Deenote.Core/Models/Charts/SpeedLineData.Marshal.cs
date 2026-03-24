#nullable enable

using Deenote.CoreB.Models.Notes;
using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.Linq;

namespace Deenote.CoreB.Models.Charts
{
    // Official json file has "lines" array, which represents a interval in which all notes has same speed,
    // but the array is actually duplicated for chart, as all note has its own speed property.
    // I dont know which property DEEMO II use to parse when game playing, I use note's property, and the
    // SpeedLine is just for serialization
    partial struct SpeedLineData
    {
        public static class Marshal
        {
            public static IEnumerable<SpeedLineData> FromNotes(IEnumerable<NoteData> notes)
                => notes
                .Adjacent()
                .Where(tpl => tpl.Item1.Speed != tpl.Item2.Speed)
                .Select(tpl => new SpeedLineData(tpl.Item2.Speed, (tpl.Item1.Time + tpl.Item2.Time) / 2));
        }
    }
}
