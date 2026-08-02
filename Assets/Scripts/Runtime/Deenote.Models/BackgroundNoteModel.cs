using System;
using System.Collections.Generic;

namespace Deenote.Models
{
    public sealed class BackgroundNoteModel : INoteTime
    {
        internal readonly NoteData _data;

        public float Time
        {
            get => _data.Time;
            set => _data.Time = value;
        }

        public List<PianoSoundData> Sounds => _data.Sounds;

        internal BackgroundNoteModel(NoteData data)
        {
            _data = data;
        }

        public BackgroundNoteModel(float time, ReadOnlySpan<PianoSoundData> pianoSounds = default)
        {
            _data = new NoteData {
                Time = time,
                Position = ChartConstraints.DefaultBackgroundPosition,
            };
            if (!pianoSounds.IsEmpty) {
                _data.Sounds.AddRange(pianoSounds.ToArray());
            }
        }

        public NoteData GetUnderlyingData() => _data;
    }
}