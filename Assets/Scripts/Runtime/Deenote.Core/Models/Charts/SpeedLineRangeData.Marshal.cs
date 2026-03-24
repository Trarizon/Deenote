#nullable enable

using Deenote.CoreB.Models.Notes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Trarizon.Library.Linq;

namespace Deenote.CoreB.Models.Charts
{
    partial struct SpeedLineRangeData
    {
        public static class Marshal
        {
            public static IEnumerable<SpeedLineRangeData> FromNotes(IEnumerable<NoteData> notes)
                => FromSpeedLineDatas(SpeedLineData.Marshal.FromNotes(notes));

            public static IEnumerable<SpeedLineRangeData> FromSpeedLineDatas(IEnumerable<SpeedLineData> speedLineDatas) 
                => speedLineDatas
                .Adjacent()
                .Where(tpl=>tpl.Item1.Speed != 1f)
                .Select(tpl=>new SpeedLineRangeData(tpl.Item1.Speed, tpl.Item1.StartTime, tpl.Item2.StartTime, tpl.Item1.WarningType));
        }
    }
}
