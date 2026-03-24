#nullable enable

using Deenote.CoreB.IO.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Deenote.CoreB.Models.Notes
{
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    partial class NoteData
    {
        /// <remarks>
        /// I found the enum defination while decompiling DEEMO, so I know its structure,
        /// but looks like that it makes no effect on note display.
        /// And in DEEMO II, the property has been removed
        /// </remarks>
        [JsonProperty("type", Order = 0)]
        [ChartSerialization(Versions = ChartSerializationVersions.DeemoV2)]
        [Obsolete("For json serialzation only, deprecated proeprty")]
        private NoteType_Legacy _serializeType = NoteType_Legacy.Hit;

        [ChartSerialization(Versions = ChartSerializationVersions.All)]
        [JsonProperty("sounds", Order = 1)]
        private List<PianoSoundData> _sounds;

        [ChartSerialization(Versions = ChartSerializationVersions.All)]
        [JsonProperty("pos", Order = 2)]
        private float _position;

        [ChartSerialization(Versions = ChartSerializationVersions.All)]
        [JsonProperty("size", Order = 3)]
        private float _size = 1f;

        [ChartSerialization(Versions = ChartSerializationVersions.All)]
        [JsonProperty("_time", Order = 4)]
        private float _time;

        /// <remarks>
        /// Unknown property
        /// </remarks>
        [ChartSerialization(Versions = ChartSerializationVersions.All)]
        [JsonProperty("shift", Order = 5)]
        private float _shift;

        /// <summary>
        /// Note speed in DEEMO II
        /// </summary>
        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("speed", Order = 6, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(1f)]
        private float _speed = 1f;

        /// <summary>
        /// Hold duration in DEEMO II
        /// </summary>
        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("duration", Order = 7)]
        private float _duration;

        /// <remarks>
        /// Unknown property, in DEEMO II.
        /// <br/>
        /// This property seems to be removed in latest DEEMO II chart
        /// </remarks>
        [ChartSerialization(Versions = ChartSerializationVersions.None)]
        [JsonProperty("vibrate", Order = 8)]
        [Obsolete("For json serialization only, deprecated property")]
        private bool _vibrate;

        /// <summary>
        /// Is swipe in DEEMO II V2
        /// </summary>
        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("swipe", Order = 9)]
        private bool _swipe;

        /// <remarks>
        /// May be speed warning in DEEMO II
        /// </remarks>
        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("warningType", Order = 10, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(WarningType.Default)]
        private WarningType _warningType = WarningType.Default;

        [ChartSerialization(Versions = ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("eventId", Order = 11)]
        [DefaultValue("")]
        private string _eventId = "";

        /// <summary>
        /// Another time property without underline prefix,
        /// In official game, this is actually a getter-only property returns _time + _shift,
        /// So we dont need to deserialize this property
        /// </summary>
        [ChartSerialization(Versions = ChartSerializationVersions.DeemoV2)]
        [JsonProperty("time", Order = 12)]
        [Obsolete("For json serialzation only, use Time instead", true)]
        private float ActualTime => _time + _shift;

        [JsonConstructor, Obsolete("For json serialzation only")]
        private NoteData(NoteType_Legacy type, List<PianoSoundData>? sounds,
            float pos, float size, float _time, float shift,
            float speed, float duration, bool vibrate, bool swipe,
            WarningType warningType, string eventId, string time)
        {
            _serializeType = type;
            _sounds = sounds ?? new();
            _position = pos;
            _size = size;
            this._time = _time;
            _shift = shift;
            _speed = speed;
            _duration = duration;
            _vibrate = vibrate;
            _swipe = swipe;
            _warningType = warningType;
        }

        public NoteData()
        {
#pragma warning disable CS0618
            _serializeType = NoteType_Legacy.Hit;
            _sounds = new();
            _position = 0;
            _size = 1;
            _time = 0;
            _shift = 0;
            _speed = 1;
            _duration = 0;
            _vibrate = false;
            _swipe = false;
            _warningType = WarningType.Default;
#pragma warning restore CS0618
        }
    }
}