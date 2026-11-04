using Deenote.Entities.Models;
using System;

namespace Deenote.GameStage.Models
{
    public readonly struct WarningNoteTimeComparable : IComparable<SpeedChangeWarningModel>
    {
        private readonly float _time;
        public WarningNoteTimeComparable(float time) => _time = time;

        public int CompareTo(SpeedChangeWarningModel other) => _time.CompareTo(other.Time);
    }
}
