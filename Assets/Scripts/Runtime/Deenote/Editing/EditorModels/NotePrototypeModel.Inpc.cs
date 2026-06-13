#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.Library;

namespace Deenote.Editing.EditorModels
{
    public sealed partial class NotePrototypeModel
    {
        public float Time
        {
            get => _time_bf;
            set {
                if (Utils.SetField(ref _time_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Time)));
                    PropertyChanged?.Invoke(this, new(nameof(PositionCoord)));
                }
            }
        }

        public float Position
        {
            get => _position_bf;
            set {
                if (Utils.SetField(ref _position_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Position)));
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
                }
                if (changed) {
                    PropertyChanged?.Invoke(this, new(nameof(PositionCoord)));
                }
            }
        }

        public float Size
        {
            get => _size_bf;
            set {
                if (Utils.SetField(ref _size_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Size)));
                }
            }
        }

        public float Duration
        {
            get => _duration_bf;
            set {
                if (Utils.SetField(ref _duration_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Duration)));
                }
            }
        }

        public float Speed
        {
            get => _speed_bf;
            set {
                if (Utils.SetField(ref _speed_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Speed)));
                }
            }
        }

        public NoteKind Kind
        {
            get => _kind_bf;
            set {
                if (Utils.SetField(ref _kind_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(Kind)));
                }
            }
        }
    }
}
