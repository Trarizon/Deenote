using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.Linq;

namespace Deenote.Models
{
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
