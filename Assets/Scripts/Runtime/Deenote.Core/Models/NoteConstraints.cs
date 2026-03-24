#nullable enable

using Deenote.CoreB.Models.Notes;
using System;

namespace Deenote.CoreB.Models
{
    public static class NoteConstraints
    {
        public const float StageMaxPosition = 2;
        public const float StageMinPosition = -2;

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
            => Math.Clamp(speed, MinNoteSpeed, MaxNoteSpeed);
    }
}
