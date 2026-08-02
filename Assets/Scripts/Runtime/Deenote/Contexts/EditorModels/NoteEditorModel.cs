using Deenote.Core.Notification;
using Deenote.CoreB.Helpers;
using Deenote.Models;
using ObservableCollections;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.Contexts.EditorModels
{
    public sealed class NoteEditorModel : INotifyPropertyChanged<NoteEditorModel>, IStageDisplayNoteEditorModel, INoteCollision
    {
        public uint Uid { get; } = INoteUnique.NextUid();

        private NoteTailEditorModel? _tail;
        internal NoteTailEditorModel? Tail => IsHold ? _tail ??= new NoteTailEditorModel(this) : null;

        public ObservableList<PianoSoundData> Sounds { get; } = new();

        #region Properties

        private float _position;
        private float _time;
        private float _size;
        private float _shift;
        private float _speed;
        private float _duration;
        private bool _vibrate;
        private NoteKind _kind;
        private WarningType _warningType;
        private string _eventId = "";

        public float Position
        {
            get => _position;
            set {
                if (_position != value) {
                    _position = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Position)));
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(PositionCoord)));
                }
            }
        }

        public float Time
        {
            get => _time;
            set {
                if (_time != value) {
                    _time = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Time)));
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(PositionCoord)));
                }
            }
        }

        public NoteCoord PositionCoord
        {
            get => new NoteCoord(_position, _time);
            set {
                // To avoid double invocation of PropertyChanged(PositionCoord)
                // we rewrite the assignement
                bool changed = false;
                if (_position != value.Position) {
                    changed = true;
                    _position = value.Position;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Position)));
                }
                if (_time != value.Time) {
                    changed = true;
                    _time = value.Time;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Time)));
                }
                if (changed) {
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(PositionCoord)));
                }
            }
        }

        public float Size
        {
            get => _size;
            set {
                if (_size != value) {
                    _size = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Size)));
                }
            }
        }

        public float Shift
        {
            get => _shift;
            set {
                if (_shift != value) {
                    _shift = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Shift)));
                }
            }
        }

        public float Speed
        {
            get => _speed;
            set {
                if (_speed != value) {
                    _speed = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Speed)));
                }
            }
        }

        public float Duration
        {
            get => _duration;
            set {
                value = Math.Max(0, value);
                if (_duration != value) {
                    var old = _duration;
                    _duration = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Duration)));
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(ActualDuration)));
                    if (old is 0 || value is 0) {
                        PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsHold)));
                    }
                }
            }
        }

        public bool Vibrate
        {
            get => _vibrate;
            set {
                if (_vibrate != value) {
                    _vibrate = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Vibrate)));
                }
            }
        }

        public NoteKind Kind
        {
            get => _kind;
            set {
                if (_kind != value) {
                    var old = _kind;
                    _kind = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(Kind)));
                    if (old is NoteKind.Swipe || value is NoteKind.Swipe) {
                        PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsHold)));
                        PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(ActualDuration)));
                    }
                }
            }
        }

        public WarningType WarningType
        {
            get => _warningType;
            set {
                if (_warningType != value) {
                    _warningType = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(WarningType)));
                }
            }
        }

        public string EventId
        {
            get => _eventId;
            set {
                if (_eventId != value) {
                    _eventId = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(EventId)));
                }
            }
        }

        #endregion

        // Link

        private NoteEditorModel? _prevLink, _nextLink;

        internal NoteEditorModel? PrevLink
        {
            get => _prevLink;
            set {
                if (_prevLink != value) {
                    _prevLink = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(PrevLink)));
                }
            }
        }

        internal NoteEditorModel? NextLink
        {
            get => _nextLink;
            set {
                if (_nextLink != value) {
                    _nextLink = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(NextLink)));
                }
            }
        }

        // 

        [MemberNotNullWhen(true, nameof(Tail))]
        public bool IsHold => Kind is not NoteKind.Swipe && Duration > 0;
        public float ActualDuration => Kind is NoteKind.Swipe ? 0 : Duration;

        // Editor properties

        private bool _isSelected;
        private int _collisionCount;

        public bool IsSelected
        {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsSelected)));
                }
            }
        }

        public int CollisionCount
        {
            get => _collisionCount;
            internal set {
                if (_collisionCount != value) {
                    _collisionCount = value;
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(CollisionCount)));
                    PropertyChanged?.Invoke(this, new PropertyEventArgs(nameof(IsCollided)));
                }
            }
        }
        int INoteCollision.CollisionCount { get => CollisionCount; set => CollisionCount = value; }

        public bool IsCollided => CollisionCount > 0;

        public event Action<NoteEditorModel, PropertyEventArgs>? PropertyChanged;

        public NoteEditorModel(NoteData data)
        {
            Sounds.AddRange(data.Sounds);
            _position = data.Position;
            _time = data.Time;
            _size = data.Size;
            _shift = data.Shift;
            _speed = data.Speed;
            _duration = data.Duration;
            _vibrate = data.Vibrate;
            _kind = data.Kind;
            _warningType = data.WarningType;
            _eventId = data.EventId;
        }

        public NoteData ToDataNonLinkInfo()
        {
            var note = new NoteData(
                sounds: Sounds.AsSpan(),
                position: _position,
                size: _size,
                time: _time,
                shift: _shift,
                speed: _speed,
                duration: _duration,
                vibrate: _vibrate,
                kind: _kind,
                warningType: _warningType,
                eventId: _eventId
            );
            return note;
        }
    }
}