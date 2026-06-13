#nullable enable

using Deenote.CoreB.Models.Notes;
using Deenote.Editing.EditorModels.Comparing;
using Deenote.Library.Collections;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Deenote.Editing.EditorModels.Helpers
{
    public static class NoteSoundsHelpers
    {
        private static readonly PianoSoundData _editorDefaultSound = new PianoSoundData(0f, 0f, 72, 0);
        public static ReadOnlySpan<PianoSoundData> EditorDefaultSounds => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _editorDefaultSound), 1);

    }
}