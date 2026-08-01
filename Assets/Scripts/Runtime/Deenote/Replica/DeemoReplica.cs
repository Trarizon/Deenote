using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Deenote.Replica
{
    internal static class DeemoReplica
    {
        // Some Animation is related to frame count rather than time
        // We use 60 fps as frame count here
        private const float Fps = 60;

        public static float DisplayFallSpeedToPlaneFallSpeed(float displayFallSpeed)
            => 3 * Mathf.Pow(1.4f, displayFallSpeed);

        public static float CalcJudgeLineHitEffectScale(float time)
        {
            // 1 - 0.02 * frameCount
            var size = 1 - 0.02f * (time * Fps);
            return Mathf.Max(0, size);
        }

        public static float CalcJudgeLineBreathingEffectAlpha(float time)
        {
            time %= 200 / Fps;

            if (time < 100 / Fps) {
                return time / (100 / Fps);
            }
            else {
                return (200 / Fps - time) / (100 / Fps);
            }
        }

        public static float CalcNoteShockwaveEffectScale(float time)
        {
            // 0.3333s

            const float GrowTime = 5f / 60f;
            const float FadeTime = 0.25f;

            if (time < GrowTime) {
                var t = time / GrowTime;
                return 1.5f * Easing.OutCubic(t);
            }
            else if (time < GrowTime + FadeTime) {
                var t = (0.25f - (time - GrowTime)) / 0.25f;
                return 1.5f * Easing.OutCubic(t);
            }
            else {
                return 0;
            }
        }

        public static (float Scale, float Alpha) CalcNoteCirclewaveEffect(float time)
        {
            // 0.5s

            const float Time1 = 1f / 6f;
            const float Time2 = 1f / 3f;

            if (time < Time1) {
                return (1 + time / (17f / 60f), time / Time1);
            }
            else if (time < Time2) {
                return (Mathf.Min(2f, 1f + time / (17f / 60f)), Mathf.InverseLerp(Time2, Time1, time));
            }
            else {
                return (1f, 0f);
            }
        }

        public static (Vector2 Scale, float Alpha) CalcNoteGlowEffect(float time)
        {
            // 0.91666666666667
            const float BaseScaleX = 0.5f;

            if (time < 5 / Fps) {
                return (
                    new Vector2(
                        x: BaseScaleX + time / (100 / Fps),
                        y: time / (10 / Fps)),
                    time / (10 / Fps)
                );
            }
            else if (time < 55 / Fps) {
                var t = 1 - (time - 5 / Fps) / (50 / Fps);
                return (
                    new Vector2(
                        x: BaseScaleX + time / (100 / Fps),
                        y: t * 0.5f),
                    t * 0.5f
                );
            }
            else {
                return (
                    new Vector2(BaseScaleX + 0.55f, y: 0f),
                    0f
                );
            }
        }
    }
}