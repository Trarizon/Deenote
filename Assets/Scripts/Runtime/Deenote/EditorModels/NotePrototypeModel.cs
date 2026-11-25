using Deenote.CoreB;
using Deenote.CoreB.Notification;
using Deenote.Entities;
using Deenote.Entities.Models;
using System;
using System.Collections.Generic;

namespace Deenote.EditorModels
{
    public sealed class NotePrototypeModel : INotifyPropertyChanged<NotePrototypeModel>, IStageTimeNode
    {
        private float _time_bf;
        public float Time
        {
            get => _time_bf;
            set {
                if (_time_bf != value) {
                    _time_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PositionCoord)));
                }
            }
        }

        private float _position_bf;
        public float Position
        {
            get => _position_bf;
            set {
                if (_position_bf != value) {
                    _position_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PositionCoord)));
                }
            }
        }

        public NoteCoord PositionCoord => new(Position, Time);

        private float _size_bf = 1f;
        public float Size
        {
            get => _size_bf;
            set {
                if (_size_bf != value) {
                    _size_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
                }
            }
        }

        private float _duration_bf;
        public float Duration
        {
            get => _duration_bf;
            set {
                if (_duration_bf != value) {
                    _duration_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHold)));
                }
            }
        }

        private float _speed_bf = 1f;
        public float Speed
        {
            get => _speed_bf;
            set {
                if (_speed_bf != value) {
                    _speed_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Speed)));
                }
            }
        }

        private NoteModel.NoteKind _kind_bf;
        public NoteModel.NoteKind Kind
        {
            get => _kind_bf;
            set {
                if (_kind_bf != value) {
                    _kind_bf = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Kind)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHold)));
                }
            }
        }
        
        public bool IsHold => Kind is not NoteModel.NoteKind.Swipe && Duration > 0;

        public event Action<NotePrototypeModel, PropertyChangedEventArgs>? PropertyChanged;

        public List<PianoSoundValueModel> Sounds { get; } = new();

        private NotePrototypeModel? _nextLink;
        private NotePrototypeModel? _prevLink;

        internal NotePrototypeModel? NextLink { get => _nextLink; }
        internal NotePrototypeModel? PrevLink { get => _prevLink; }

        public NotePrototypeModel()
        {
        }

        public NoteModel CreateAt(NoteCoord coord)
        {
            var data = ToDataNonLinkInfo();
            data.PositionCoord += coord;
            return data;
        }

        public NoteModel ToDataNonLinkInfo()
        {
            var data = new NoteModel {
                Time = Time,
                Position = Position,
                Size = Size,
                Duration = Duration,
                Speed = Speed,
                Kind = Kind,
            };
            return data;
        }

        public void FromDataNonLinkInfo(NoteModel noteModel, NoteCoord baseCoord)
        {
            Time = noteModel.Time - baseCoord.Time;
            Position = noteModel.Position - baseCoord.Position;
            Size = noteModel.Size;
            Duration = noteModel.Duration;
            Speed = noteModel.Speed;
            Kind = noteModel.Kind;
            Sounds.Replace(noteModel.Sounds);
        }

        public void CloneTo(NotePrototypeModel target, bool cloneSounds = true)
        {
            target.Time = Time;
            target.Position = Position;
            target.Size = Size;
            target.Duration = Duration;
            target.Speed = Speed;
            target.Kind = Kind;
            if (cloneSounds) {
                target.Sounds.Replace(Sounds);
            }
        }

        internal static class Marshal
        {
            public static void Link(NotePrototypeModel prev, NotePrototypeModel next)
            {
                prev._nextLink = next;
                next._prevLink = prev;
            }

            public static void UnlinkNext(NotePrototypeModel note)
            {
                if (note._nextLink is not null)
                    note._nextLink._prevLink = null;
                note._nextLink = null;
            }

            public static void UnlinkPrev(NotePrototypeModel note)
            {
                if (note._prevLink is not null)
                    note._prevLink._nextLink = null;
                note._prevLink = null;
            }
        }
    }
}