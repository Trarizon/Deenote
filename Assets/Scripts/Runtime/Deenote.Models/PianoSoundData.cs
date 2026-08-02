using Newtonsoft.Json;
using System;

namespace Deenote.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct PianoSoundData
    {
        [JsonProperty("w", Order = 0)] public float Delay;
        [JsonProperty("d", Order = 1)] public float Duration;
        [JsonProperty("p", Order = 2)] public int Pitch;
        [JsonProperty("v", Order = 3)] public int Velocity;

        [JsonConstructor]
        public PianoSoundData(float delay, float duration, int pitch, int velocity)
        {
            Delay = delay;
            Duration = duration;
            Pitch = pitch;
            Velocity = velocity;
        }

        #region Equality

        public override readonly bool Equals(object obj)
            => obj is PianoSoundData data && data == this;
        public override readonly int GetHashCode()
            => HashCode.Combine(Delay, Duration, Pitch, Velocity);

        public static bool operator ==(PianoSoundData left, PianoSoundData right)
            => left.Delay == right.Delay && left.Duration == right.Duration
            && left.Pitch == right.Pitch && left.Velocity == right.Velocity;
        public static bool operator !=(PianoSoundData left, PianoSoundData right)
            => !(left == right);

        #endregion
    }
}