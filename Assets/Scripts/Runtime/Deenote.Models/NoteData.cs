using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Deenote.Models
{
    public sealed partial class NoteData : INoteTime, INoteLink<NoteData>
    {
        public List<PianoSoundData> Sounds => _sounds;
        public float Position { get => _position; set => _position = value; }
        public float Time { get => _time; set => _time = value; }
        public float Size { get => _size; set => _size = value; }
        /// <remarks>
        /// The value is not guaranteed to be 0 if <see cref="IsHold"/> is <see langword="false"/>,
        /// use <see cref="ActualDuration"/> to get the actual duration
        /// </remarks>
        public float Duration { get => _duration; set => _duration = value; }
        public NoteKind Kind
        {
            get => _swipe ? NoteKind.Swipe : _slide ? NoteKind.Slide : NoteKind.Click;
        }
        public float Speed { get => _speed; set => _speed = value; }
        public float Shift { get => _shift; set => _shift = value; }
        public string EventId { get => _eventId; set => _eventId = value; }
        public WarningType WarningType { get => _warningType; set => _warningType = value; }
#pragma warning disable CS0618
        public bool Vibrate { get => _vibrate; set => _vibrate = value; }
#pragma warning restore CS0618

        internal bool _slide;
        internal NoteData? _prevLink;
        internal NoteData? _nextLink;

        public NoteData? PrevLink => _prevLink;

        public NoteData? NextLink => _nextLink;

        public NoteCoord PositionCoord
        {
            get => new NoteCoord(Position, Time);
            set => (Position, Time) = (value.Position, value.Time);
        }

        public bool IsSwipe { get => _swipe; set => _swipe = value; }
        public bool IsSlide => !_swipe && _slide;
        public bool IsHold => !_swipe && Duration > 0;
        public bool HasSounds => _sounds.Count > 0;
        /// <summary>
        /// Get the actual duration, which ensures note is a hold note if return value &gt; 0
        /// </summary>
        /// <returns>The actual hold duration, 0 if the note is not a hold note</returns>
        public float ActualDuration => _swipe ? Duration : 0;
        public float EndTime => _swipe ? Time + Duration : Time;

        NoteData? INoteLink<NoteData>.NextLink { get => NextLink; set => _nextLink = value; }
        NoteData? INoteLink<NoteData>.PrevLink { get => PrevLink; set => _prevLink = value; }

        internal void SetKind(NoteKind kind)
        {
            (_swipe, _slide) = kind switch {
                NoteKind.Click => (false, false),
                NoteKind.Slide => (false, true),
                NoteKind.Swipe => (true, false),
                _ => throw new SwitchExpressionException(kind),
            };
        }

        public void SetRawIsSlide()
        {
            _slide = true;
        }

        public NoteData CloneNonLinkInfo(bool cloneSounds = true)
        {
            var note = new NoteData();
            CloneToNonLinkInfo(note, cloneSounds);
            return note;
        }

        public void CloneToNonLinkInfo(NoteData other, bool cloneSounds = true)
        {
            other.Position = Position;
            other.Time = Time;
            other.Size = Size;
            other.Duration = Duration;
            other.Speed = Speed;
            other.Shift = Shift;
            other.WarningType = WarningType;
            other.Vibrate = Vibrate;
            other._slide = _slide;
            other._swipe = _swipe;
            if (cloneSounds) {
                other.Sounds.Clear();
                other.Sounds.AddRange(Sounds);
            }
        }
    }
}