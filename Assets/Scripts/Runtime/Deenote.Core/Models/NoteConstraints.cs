#nullable enable

using Deenote.CoreB.Models.Notes;
using System;

namespace Deenote.CoreB.Models
{
    public static class NoteConstraints
    {
        public const float DefaultSize = 1;
        public const float DefaultSpeed = 1;

        public const float DefaultBackgroundPosition = 12f;
        public const float DefaultWarningNotePosition = 4f;

        public const float StageMaxPosition = 2;
        public const float StageMinPosition = -2;
        public const float StageMaxPositionWidth = StageMaxPosition - StageMinPosition;

        private const float MinNoteSize = 0.1f;
        //private const float MaxNoteSize = 5f;
        private const float MinNoteSpeed = 0.1f;
        private const float MaxNoteSpeed = 100f;

        public static bool IsVisibleOnStage(this NoteData note)
            => note.Position is >= StageMinPosition and <= StageMaxPosition;

        public static float ClampTime(float time, float maxTime)
            => Math.Clamp(time, 0, maxTime);

        public static float ClampPosition(float position)
            => Math.Clamp(position, StageMinPosition, StageMaxPosition);

        public static float ClampSize(float size)
            => Math.Max(size, MinNoteSize);

        public static float ClampSpeed(float speed)
        {
            if (speed <= 0f)
                return MinNoteSpeed;
            return Math.Min(speed, MaxNoteSpeed);
        }

        public static float ClampDuration(float v) 
            => Math.Max(0f, v);
    }
}
