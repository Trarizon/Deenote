#nullable enable

using Deenote.CoreB.Models.Notes;
using System.Collections.Generic;

namespace Deenote.CoreB.Models.Charts
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
    }
}
