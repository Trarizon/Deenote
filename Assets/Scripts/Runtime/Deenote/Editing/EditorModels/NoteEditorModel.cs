#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class NoteEditorModel : ObservableObject, IGameStageNoteNode, IGameNote, INoteLink, INoteTime, ICollidableNote
    {
        public uint Uid { get; }

        private NoteTailEditorModel? _tail;
        internal NoteTailEditorModel? Tail => Duration > 0 ? _tail ??= new NoteTailEditorModel(this) : null;

        public List<PianoSoundData> Sounds { get; } = new();
        [NotifyPropertyChangedFor(nameof(PositionCoord))]
        [ObservableProperty] float _position;
        [NotifyPropertyChangedFor(nameof(PositionCoord), nameof(EndTime))]
        [ObservableProperty] float _time;
        [ObservableProperty] float _size;
        [ObservableProperty] float _shift;
        [ObservableProperty] float _speed;
        /// <summary>
        /// The value is not guaranteed to be 0 if <see cref="IsHold"/> is <see langword="false"/>,
        /// use <see cref="ActualDuration"/> to get the actual duration
        /// </summary>
        [NotifyPropertyChangedFor(nameof(ActualDuration), nameof(EndTime), nameof(IsHold))]
        [ObservableProperty] float _duration;
        [ObservableProperty] bool _vibrate;
        [ObservableProperty] NoteKind _kind;
        [ObservableProperty] WarningType _warningType;
        [ObservableProperty] string _eventId = "";

        internal INoteLink? PrevLink { get; set; }
        internal INoteLink? NextLink { get; set; }

        INoteLink? INoteLink.NextLink { get => NextLink; set => NextLink = value; }
        INoteLink? INoteLink.PrevLink { get => PrevLink; set => PrevLink = value; }


        public bool IsSlide => Kind is NoteKind.Slide;
        public bool IsSwipe => Kind is NoteKind.Swipe;
        public bool IsHold => Kind is not NoteKind.Swipe && Duration > 0;
        public float ActualDuration => Kind is NoteKind.Swipe ? 0 : Duration;
        public bool HasSounds => Sounds.Count > 0;
        public float EndTime => Time + ActualDuration;
        public NoteCoord PositionCoord
        {
            get => new(Position, Time);
            set => SetProperty(PositionCoord, value, v => (Position, Time) = (v.Position, v.Time));
        }

        public bool IsSelected { get; set; }
        public bool IsCollided => CollisionCount > 0;
        public int CollisionCount { get; internal set; }

        int ICollidableNote.CollisionCount { get => CollisionCount; set => CollisionCount = value; }

        public bool IsComboNode => Duration <= 0;

        public NoteEditorModel(NoteData model)
        {
            Uid = INoteUnique.GetUid();

            Sounds.AddRange(model.Sounds);
            _position = model.Position;
            _time = model.Time;
            _size = model.Size;
            _shift = model.Shift;
            _speed = model.Speed;
            _duration = model.Duration;
            _vibrate = model.Vibrate;
            _kind = model.Kind;
            _warningType = model.WarningType;
            _eventId = model.EventId;

            if (_duration > 0) {
                _tail = new NoteTailEditorModel(this);
            }
        }

        partial void OnDurationChanged(float oldValue, float newValue)
        {
            switch (oldValue, newValue) {
                case (0, > 0):
                    _tail = new NoteTailEditorModel(this);
                    break;
                case ( > 0, 0):
                    _tail = null;
                    break;
                default:

                    break;
            }
        }

        public NoteData ToDataNonLinkInfo()
        {
            var note = new NoteData {
                Duration = Duration,
                EventId = EventId,
                IsSwipe = IsSwipe,
                //Kind = Kind,
                //NextLink = NextLink,
                Position = Position,
                //PrevLink = PrevLink,
                Shift = Shift,
                Size = Size,
                //Sounds = Sounds,
                Speed = Speed,
                Time = Time,
                Vibrate = Vibrate,
                WarningType = WarningType,
            };
            note.SetRawIsSlide();
            return note;
        }
    }
}
