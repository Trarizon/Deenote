using UnityEngine;

namespace Deenote.Models
{
    public struct Tempo
    {
        public static Tempo PositiveInfinity => new Tempo(0, float.PositiveInfinity);

        public const float MaxBpm = 1200f;
        public const float MinBeatLineInterval = 60 / MaxBpm;

        /// <remarks>
        /// 0 or positive number.
        /// </remarks>
        public float Bpm;
        public float StartTime;

        public readonly float BeatInterval => 60f / Bpm;

        public Tempo(float bpm, float startTime)
        {
            Bpm = bpm;
            StartTime = startTime;
        }

        public readonly int GetBeatIndex(float time)
        {
            if (Bpm == 0f) {
                return time >= StartTime ? 0 : -1;
            }

            float timeOffsetToTempo = time - StartTime;
            float interval = 60 / Bpm;

            var fvalue = timeOffsetToTempo / interval;
            var result = Mathf.FloorToInt(fvalue);
            if (Mathf.Approximately(result + 1, fvalue))
                return result + 1;
            else
                return result;
        }

        public readonly int GetCeilingBeatIndex(float time)
        {
            if (Bpm == 0f) {
                return time > StartTime ? 1 : 0;
            }

            float timeOffsetToTempo = time - StartTime;
            float interval = 60 / Bpm;

            var fvalue = timeOffsetToTempo / interval;
            var result = Mathf.CeilToInt(fvalue);
            if (Mathf.Approximately(result - 1, fvalue))
                return result - 1;
            else
                return result;
        }

        public readonly float GetBeatTime(int beatIndex)
        {
            if (Bpm == 0f) {
                return beatIndex switch {
                    > 0 => float.PositiveInfinity,
                    0 => StartTime,
                    < 0 => float.NegativeInfinity,
                };
            }

            return StartTime + beatIndex * 60 / Bpm;
        }

        public readonly float GetSubdivisionTime(float beatIndex)
        {
            if (Bpm == 0f) {
                return beatIndex switch {
                    > 0f => float.PositiveInfinity,
                    < 0f => float.NegativeInfinity,
                    _ => StartTime,
                };
            }

            return StartTime + beatIndex * 60f / Bpm;
        }
    }
}
