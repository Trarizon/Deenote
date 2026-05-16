#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Library.Collections;
using System.Collections.Generic;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class NotePrototypeModel : ObservableObject, IGameNote, INoteLink<NotePrototypeModel>
    {
        public uint Uid { get; }

        [ObservableProperty] float _time;
        [ObservableProperty] float _position;
        [ObservableProperty] float _size = NoteConstraints.DefaultSize;
        [ObservableProperty] float _duration;
        [ObservableProperty] float _speed = NoteConstraints.DefaultSpeed;
        [ObservableProperty] NoteKind _kind;

        public List<PianoSoundData> Sounds { get; } = new();

        public NoteCoord PositionCoord
        {
            get => new(Position, Time);
            set => (Position, Time) = (value.Position, value.Time);
        }

        internal NotePrototypeModel? NextLink { get; set; }
        internal NotePrototypeModel? PrevLink { get; set; }

        NotePrototypeModel? INoteLink<NotePrototypeModel>.NextLink { get => NextLink; set => NextLink = value; }
        NotePrototypeModel? INoteLink<NotePrototypeModel>.PrevLink { get => PrevLink; set => PrevLink = value; }

        public NotePrototypeModel()
        {
            Uid = INoteUnique.GetUid();
        }

        public NoteData CreateAt(NoteCoord coord)
        {
            var data = ToDataNonLinkInfo();
            data.Position += coord.Position;
            data.Time += coord.Time;
            return data;
        }

        public NoteData ToDataNonLinkInfo()
        {
            var data = new NoteData {
                Duration = Duration,
                IsSwipe = Kind is NoteKind.Swipe,
                Position = Position,
                Time = Time,
                Size = Size,
            };
            data.Sounds.AddRange(Sounds.AsSpan());
            return data;
        }

        public NotePrototypeModel Clone(bool cloneSounds = true)
        {
            var model = new NotePrototypeModel();
            CloneTo(model, cloneSounds);
            return model;
        }

        public void CloneTo(NotePrototypeModel other, bool cloneSounds = true)
        {
            other.Time = Time;
            other.Position = Position;
            other.Size = Size;
            other.Duration = Duration;
            other.Speed = Speed;
            other.Kind = Kind;
            if (cloneSounds) {
                other.Sounds.Replace(Sounds.AsSpan());
            }
        }

        public void FromDataNonLinkInfo(NoteData data, NoteCoord baseCoord = default)
        {
            Time = data.Time - baseCoord.Time;
            Position = data.Position - baseCoord.Position;
            Size = data.Size;
            Duration = data.Duration;
            Speed = data.Speed;
            Kind = data.Kind;
            Sounds.Replace(data.Sounds.AsSpan());
        }
    }
}
