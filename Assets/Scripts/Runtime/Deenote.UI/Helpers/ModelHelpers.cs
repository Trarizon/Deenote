#nullable enable

using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels;
using System;

namespace Deenote.UI.Helpers
{
    internal static class  ModelHelpers
    {
        private static readonly string[] _pianoNoteNames = new[] {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };

        public static string ToPitchDisplayString(this in PianoSoundData sound) {
            int octave = Math.DivRem(sound.Pitch, 12, out var rem) - 2;
            return $"{_pianoNoteNames[rem]}{octave}";
        }

        public static bool HasSameSounds(ReadOnlySpan<NoteEditorModel> notes)
        {
            var first = notes[0].Sounds;

            for (int i = 1; i < notes.Length; i++) {
                var sounds = notes[i].Sounds;
                if (first.Count != sounds.Count)
                    return false;

                for (int j = 0; j < sounds.Count; j++) {
                    if (first[j] != sounds[j])
                        return false;
                }
            }
            return true;

        }
    }
}
