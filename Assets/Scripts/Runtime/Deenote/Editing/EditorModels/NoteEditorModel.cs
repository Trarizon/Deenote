#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using System;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    public sealed class NoteEditorModel : INotifyPropertyChanged<NoteEditorModel>, IGameStageNoteNode, IGameNote, INoteLink, INoteTime, ICollidableNote
    {
        public uint Uid { get; }

        private NoteTailEditorModel? _tail;
        internal NoteTailEditorModel? Tail => Duration > 0 ? _tail ??= new NoteTailEditorModel(this) : null;

        // REFACTOR: sounds需要Notifiable
        public List<PianoSoundData> Sounds { get; } = new();

        private float _position_bf;
        public float Position
        {
            get => _position_bf;
            set {
                if (_position_bf != value) {
                    _position_bf = value;
                    // Mirrored in PositionCoord, remember to update PositionCoord if changed
                    PropertyChanged?.Invoke(this, new(nameof(Position)));
                    PropertyChanged?.Invoke(this, new(nameof(PositionCoord)));
                }
            }
        }

        private float _time_bf;
        public float Time
        {
            get => _time_bf;
            set {
                if (_time_bf != value) {
                    _time_bf = value;
                    // Mirrored in PositionCoord, remember to update PositionCoord if changed
                    PropertyChanged?.Invoke(this, new(nameof(Time)));
                    PropertyChanged?.Invoke(this, new(nameof(EndTime)));
                    PropertyChanged?.Invoke(this, new(nameof(PositionCoord)));
                }
            }
        }

        public NoteCoord PositionCoord
        {
            get => new(Position, Time);
            set {
                // To avoid double invocation of PropertyChanged(PositionCoord)
                // we rewrite the assignement
                bool changed = false;
                if (_position_bf != value.Position) {
                    changed = true;
                    _position_bf = value.Position;
                    PropertyChanged?.Invoke(this, new(nameof(Position)));
                }
                if (_time_bf != value.Time) {
                    changed = true;
                    _time_bf = value.Time;
                    PropertyChanged?.Invoke(this, new(nameof(Time)));
                    PropertyChanged?.Invoke(this, new(nameof(EndTime)));
                }
                if (changed) {
                    PropertyChanged?.Invoke(this, new(nameof(PositionCoord)));
                }
            }
        }

        private float _size_bf;
        public float Size
        {
            get => _size_bf;
            set {
                if (_size_bf != value) {
                    _size_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(Size)));
                }
            }
        }

        private float _shift_bf;
        public float Shift
        {
            get => _shift_bf;
            set {
                if (_shift_bf != value) {
                    _shift_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(Shift)));
                }
            }
        }

        private float _speed_bf;
        public float Speed
        {
            get => _speed_bf;
            set {
                if (_speed_bf != value) {
                    _speed_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(Speed)));
                }
            }
        }

        private float _duration_bf;
        /// <summary>
        /// The value is not guaranteed to be 0 if <see cref="IsHold"/> is <see langword="false"/>,
        /// use <see cref="ActualDuration"/> to get the actual duration
        /// </summary>
        public float Duration
        {
            get => _duration_bf;
            set {
                if (_duration_bf != value) {
                    var oldValue = _duration_bf;
                    _duration_bf = value;
                    switch (oldValue, value) {
                        case (0, > 0):
                            _tail = new NoteTailEditorModel(this);
                            break;
                        case ( > 0, 0):
                            _tail = null;
                            break;
                        default:
                            break;
                    }
                    PropertyChanged?.Invoke(this, new(nameof(Duration)));
                    PropertyChanged?.Invoke(this, new(nameof(ActualDuration)));
                    PropertyChanged?.Invoke(this, new(nameof(EndTime)));
                    PropertyChanged?.Invoke(this, new(nameof(IsHold)));
                    PropertyChanged?.Invoke(this, new(nameof(IsComboNode)));
                }
            }
        }

        private bool _vibrate_bf;
        public bool Vibrate
        {
            get => _vibrate_bf;
            set {
                if (_vibrate_bf != value) {
                    _vibrate_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(Vibrate)));
                }
            }
        }

        private NoteKind _kind_bf;
        public NoteKind Kind
        {
            get => _kind_bf;
            set {
                if (_kind_bf != value) {
                    _kind_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(Kind)));
                    PropertyChanged?.Invoke(this, new(nameof(IsSlide)));
                    PropertyChanged?.Invoke(this, new(nameof(IsSwipe)));
                    PropertyChanged?.Invoke(this, new(nameof(IsHold)));
                    PropertyChanged?.Invoke(this, new(nameof(ActualDuration)));
                }
            }
        }

        private WarningType _warningType_bf;
        public WarningType WarningType
        {
            get => _warningType_bf;
            set {
                if (_warningType_bf != value) {
                    _warningType_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(WarningType)));
                }
            }
        }

        private string _eventId_bf = "";
        public string EventId
        {
            get => _eventId_bf;
            set {
                if (_eventId_bf != value) {
                    _eventId_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(EventId)));
                }
            }
        }

        private INoteLink? _prevLink_bf, _nextLink_bf;
        internal INoteLink? PrevLink
        {
            get => _prevLink_bf;
            set {
                if (_prevLink_bf != value) {
                    _prevLink_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(PrevLink)));
                }
            }
        }
        internal INoteLink? NextLink
        {
            get => _nextLink_bf;
            set {
                if (_nextLink_bf != value) {
                    _nextLink_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(NextLink)));
                }
            }
        }

        INoteLink? INoteLink.NextLink { get => NextLink; set => NextLink = value; }
        INoteLink? INoteLink.PrevLink { get => PrevLink; set => PrevLink = value; }


        public bool IsSlide => Kind is NoteKind.Slide;
        public bool IsSwipe => Kind is NoteKind.Swipe;
        public bool IsHold => Kind is not NoteKind.Swipe && Duration > 0;
        public float ActualDuration => Kind is NoteKind.Swipe ? 0 : Duration;
        public bool HasSounds => Sounds.Count > 0;
        public float EndTime => Time + ActualDuration;

        private bool _isSelected_bf;
        public bool IsSelected
        {
            get => _isSelected_bf;
            set {
                if (_isSelected_bf != value) {
                    _isSelected_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
                }
            }
        }

        private int _collisionCount_bf;
        public int CollisionCount
        {
            get => _collisionCount_bf;
            internal set {
                if (_collisionCount_bf != value) {
                    _collisionCount_bf = value;
                    PropertyChanged?.Invoke(this, new(nameof(CollisionCount)));
                    PropertyChanged?.Invoke(this, new(nameof(IsCollided)));
                }
            }
        }
        public bool IsCollided => CollisionCount > 0;

        int ICollidableNote.CollisionCount { get => CollisionCount; set => CollisionCount = value; }

        public bool IsComboNode => Duration <= 0;

        public event Action<NoteEditorModel, PropertyEventArgs>? PropertyChanged;

        public NoteEditorModel(NoteData model)
        {
            Uid = INoteUnique.GetUid();

            Sounds.AddRange(model.Sounds);
            Position = model.Position;
            Time = model.Time;
            Size = model.Size;
            Shift = model.Shift;
            Speed = model.Speed;
            Duration = model.Duration;
            Vibrate = model.Vibrate;
            Kind = model.Kind;
            WarningType = model.WarningType;
            EventId = model.EventId;

            if (Duration > 0) {
                _tail = new NoteTailEditorModel(this);
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
            if (note.IsSlide) {
                note.SetRawIsSlide();
            }
            return note;
        }
    }
}
