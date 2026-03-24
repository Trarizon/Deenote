#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;

namespace Deenote.CoreB.Models.Notes
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
                Position = 12f,
            };
            if (!pianoSounds.IsEmpty) {
                _data.Sounds.AddRange(pianoSounds);
            }
        }

        public NoteData GetUnderlyingData() => _data;
    }
}
