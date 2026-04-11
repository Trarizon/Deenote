#nullable enable

using Deenote.CoreB.Models;
using System;

namespace Deenote.Editing.EditorModels.Helpers
{
    internal static class TempoHelpers
    {
        /// <returns>Range: [-1, Count)</returns>
        public static int GetTempoIndex(this ReadOnlySpan<Tempo> tempos, float time)
        {
            int i = 0;
            for (; i < tempos.Length; i++) {
                if (tempos[i].StartTime > time)
                    break;
            }
            return i - 1;
        }

        /// <returns>Range: [0, Count]</returns>
        public static int GetCeilingTempoIndex(this ReadOnlySpan<Tempo> tempos, float time)
        {
            int i = 0;
            for (; i < tempos.Length; i++) {
                if (tempos[i].StartTime >= time)
                    break;
            }
            return i;
        }

        /// <returns>
        /// if <paramref name="index"/> less than 0, return a <see cref="Tempo"/>
        /// with 0 bpm and 0 start time
        /// </returns>
        public static Tempo GetActualTempo(this ReadOnlySpan<Tempo> tempos, int index)
            => index < 0 ? new Tempo(0f, 0f) : tempos[index];

    }
}
