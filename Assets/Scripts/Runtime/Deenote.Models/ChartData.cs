using Deenote.CoreB.Helpers;
using Deenote.Models.Helpers;
using System.Collections.Generic;

namespace Deenote.Models
{
    public sealed partial class ChartData
    {
        public float Speed { get => _speed; set => _speed = value; }
        public int RemapMinVolume { get => _remapVMin; set => _remapVMin = value; }
        public int RemapMaxVolume { get => _remapVMax; set => _remapVMax = value; }

        public List<NoteData> Notes => _notes;

        public List<SpeedLineRangeData> SpeedLines => _lines;

        public ChartData(float speed, int remapMinVolume, int remapMaxVolume)
        {
            Speed = speed;
            RemapMinVolume = remapMinVolume;
            RemapMaxVolume = remapMaxVolume;

            _notes = new();
            _lines = new();
        }

        public ChartData Clone()
        {
            var chart = new ChartData(0, 0, 0);
            CloneTo(chart);
            return chart;
        }

        public void CloneTo(ChartData other)
        {
            other.Speed = Speed;
            other.RemapMinVolume = RemapMinVolume;
            other.RemapMaxVolume = RemapMaxVolume;

            other.Notes.EnsureCapacity(Notes.Count);
            other.Notes.Replace(Notes.AsSpan());
            NoteLinkHelpers.CloneLinkInfos(Notes.AsSpan(), other.Notes.AsSpan());

            other.SpeedLines.EnsureCapacity(SpeedLines.Count);
            other.SpeedLines.Replace(SpeedLines.AsSpan());
        }
    }
}