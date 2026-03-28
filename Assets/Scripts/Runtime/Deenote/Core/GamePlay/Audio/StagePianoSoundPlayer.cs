#nullable enable

using Deenote.Audio;
using Deenote.CoreB.Models.Notes;
using System;

namespace Deenote.Core.GamePlay.Audio
{
    public sealed class StagePianoSoundPlayer
    {
        private readonly PianoSoundSource _source;

        public float Volume { get; internal set; }
        public float Speed { get; internal set; }

        public StagePianoSoundPlayer(PianoSoundSource source)
        {
            _source = source;
        }

        // TODO: split playback volume and velocity
        public void PlaySound(PianoSoundData sound)
        {
            if (Volume == 0f) return;

            _ = _source.PlaySoundAsync(sound.Pitch, (int)(sound.Velocity * Volume), sound.Duration, sound.Delay, Speed);
        }

        public void PlaySounds(ReadOnlySpan<PianoSoundData> sounds)
        {
            if (Volume == 0f) return;

            foreach (var sound in sounds) {
                PlaySound(sound);
            }
        }
    }
}