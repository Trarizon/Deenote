using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Deenote.Project.Models.Datas
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class NoteData
    {
        #region Serialize Members

        [Obsolete("Unknown property, for json serialization only.")]
        [SerializeField]
        [JsonProperty("type")]
        public NoteType Type;

        [SerializeField]
        [JsonProperty("sounds")]
        private List<PianoSoundData> _sounds;
        public List<PianoSoundData> Sounds
        {
            get => _sounds ??= new();
            init => _sounds = value;
        }

        [SerializeField]
        [JsonProperty("pos")]
        public float Position;

        [SerializeField]
        [JsonProperty("size")]
        public float Size;

        [SerializeField]
        [JsonProperty("_time")]
        public float Time;

        /// <summary>
        /// Unknown property
        /// </summary>
        [SerializeField]
        [JsonProperty("shift")]
        public float Shift;

        /// <summary>
        /// Note speed in DEEMO II
        /// </summary>
        [SerializeField]
        [JsonProperty("speed")]
        public float Speed;

        /// <summary>
        /// Hold duration in DEEMO II
        /// </summary>
        [SerializeField]
        [JsonProperty("duration")]
        public float Duration;

        /// <summary>
        /// Unknown property, in DEEMO II
        /// </summary>
        [SerializeField]
        [JsonProperty("vibrate")]
        public bool Vibrate;

        /// <summary>
        /// Is swipe in DEEMO II V2
        /// </summary>
        [SerializeField]
        [JsonProperty("swipe")]
        public bool IsSwipe;

        /// <summary>
        /// Unknown property in DEEMO II
        /// </summary>
        [SerializeField]
        [JsonProperty("warningType")]
        public WarningType WarningType;

        [SerializeField]
        [JsonProperty("eventId")]
        [DefaultValue("")]
        public string EventId;

        /// <summary>
        /// Another time property without underline prefix,
        /// We treat Time(_time in json) as the real time of the note,
        /// Make this property invalid if the usage of "time" in json is unclear 
        /// </summary>
        [Obsolete("For deserialization only, use Time instead", true)]
        [SerializeField]
        [JsonProperty("time")]
        public float TimeDuplicate { get => Time; set { } }

        #endregion

        [SerializeField]
        private bool _isSlide;
        [SerializeField]
        private NoteData _nextLinkNote;
        [SerializeField]
        private NoteData _prevLinkNote;

        public bool IsSlide
        {
            get => _isSlide;
            set {
                if (value == _isSlide)
                    return;
                if (!value) {
                    if (_prevLinkNote is not null)
                        _prevLinkNote.NextLink = _nextLinkNote;
                    if (_nextLinkNote is not null)
                        _nextLinkNote.PrevLink = _prevLinkNote;
                }
                _prevLinkNote = _nextLinkNote = null;
                _isSlide = value;
                return;
            }
        }

        public NoteData PrevLink
        {
            get => _prevLinkNote;
            set {
                if (value != null)
                    _isSlide = true;
                _prevLinkNote = value;
            }
        }

        public NoteData NextLink
        {
            get => _nextLinkNote;
            set {
                if (value != null)
                    _isSlide = true;
                _nextLinkNote = value;
            }
        }

        public bool HasSound => Sounds?.Count > 0;

        public bool IsHold => !IsSwipe && Duration > 0f;

        public float EndTime => Time + Duration;

        public bool IsVisible => Position is >= -MainSystem.Args.StageMaxPosition and <= MainSystem.Args.StageMaxPosition;

        public NoteCoord PositionCoord
        {
            get => new(Position, Time);
            set => (Position, Time) = (value.Position, value.Time);
        }

        public NoteData Clone()
        {
#pragma warning disable CS0618 // Clone all
            return new NoteData {
                Type = Type,
                Sounds = Sounds.Select(s => s.Clone()).ToList(),
                Position = Position,
                Size = Size,
                Time = Time,
                Shift = Shift,
                Speed = Speed,
                Duration = Duration,
                Vibrate = Vibrate,
                IsSwipe = IsSwipe,
                WarningType = WarningType,
                EventId = EventId,
                IsSlide = IsSlide,
            };
#pragma warning restore CS0618
        }

        [Obsolete("For json serialization only")]
        public enum NoteType { Hit = 0, Slide = 1, }
    }
}
