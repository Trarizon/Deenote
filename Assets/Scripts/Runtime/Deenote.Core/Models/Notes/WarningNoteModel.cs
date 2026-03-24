#nullable enable

using System.Diagnostics;

namespace Deenote.CoreB.Models.Notes
{
    public sealed class WarningNoteModel : INoteTime
    {
        internal readonly NoteData _data;

        public float Time => _data.Time;

        internal WarningNoteModel(NoteData data)
        {
            Debug.Assert(data.WarningType is WarningType.SpeedChange);
            _data = data;
        }

        public WarningNoteModel(float time)
        {
            _data = new NoteData {
                Time = time,
                Position = 4,
                WarningType = WarningType.SpeedChange
            };
        }

        public NoteData GetUnderlyingData() => _data;
    }
}
