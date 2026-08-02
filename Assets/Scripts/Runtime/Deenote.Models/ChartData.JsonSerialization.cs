using Deenote.Models.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.Library.Linq;

namespace Deenote.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    partial class ChartData
    {
        [JsonProperty("speed", Order = 0)]
        private float _speed;

        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("oriVMin", Order = 1), Obsolete("For serialzation only")]
        private int _SerializeMinVolume => _notes.SelectMany(n => n.Sounds, (n, s) => s.Velocity).MinOrDefault();

        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("oriVMax", Order = 2), Obsolete("For serialzation only")]
        private int _SerializeMaxVolume => _notes.SelectMany(n => n.Sounds, (n, s) => s.Velocity).MaxOrDefault();

        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("remapVMin", Order = 3)]
        private int _remapVMin;

        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("remapVMax", Order = 4)]
        public int _remapVMax;

        [JsonProperty("notes", Order = 5)]
        private List<NoteData> _notes;

        [JsonProperty("links", Order = 6), Obsolete("For serialzation only")]
        private IEnumerable<NoteLinkIterator> _SerializeLinks
            => _notes
                .Where(n => n.Kind is NoteKind.Slide && n.PrevLink is null)
                .Select(n => new NoteLinkIterator(n));


        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("lines", Order = 7)]
        private List<SpeedLineRangeData> _lines;


        [JsonConstructor]
        internal ChartData(float speed, int oriVMin, int oriVMax, int remapVMin, int remapVMax,
            List<NoteData>? notes,
            IEnumerable<NoteLinkIterator.Deserializer>? links,
            List<SpeedLineRangeData>? lines)
        {
            _speed = speed;
            _remapVMin = remapVMin;
            _remapVMax = remapVMax;
            _notes = notes ?? new List<NoteData>();
            _lines = lines ?? new List<SpeedLineRangeData>();

            if (links is not null) {
                foreach (var link in links) {
                    NoteData? prev = null;
                    foreach (var note in link.Notes) {
                        note._slide = true;
                        note._prevLink = prev;
                        if (prev != null)
                            prev._nextLink = note;
                        prev = note;
                    }
                }
            }
        }

        public static bool TryParse(string json, [NotNullWhen(true)] out ChartData? chart)
        {
            chart = ChartJsonSerializer.Deserialize(json);
            return chart is not null;
        }

        public string ToJsonString() => ToJsonString(ChartVersion.DeemoIIV2);

        public string ToJsonString(ChartVersion version)
        {
            return ChartJsonSerializer.Serialize(this, version);
        }
    }
}