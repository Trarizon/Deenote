using Cysharp.Threading.Tasks;
using Deenote.CoreB;
using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Deenote.Replica
{
    internal static class DeemoReplica
    {
        public static float DisplayFallSpeedToPlaneFallSpeed(float displayFallSpeed)
            => 3 * Mathf.Pow(1.4f, displayFallSpeed);

        // Stage Animations

        // Some Animation is related to frame count rather than time
        // We use 60 fps as frame count here
        private const float Fps = 60;

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

        // PianoSound

        private static readonly int[] _pitches = { 24, 38, 43, 48, 53, 57, 60, 64, 67, 71, 74, 77, 81, 84, 89, 95 };
        private static readonly int[] _velocities = { 38, 63, 111, 127 };

        private static ReadOnlySpan<T> GetNearestClipsByPitch<T>(int pitch, out int resultPitch, ReadOnlySpan<T> clips)
        {
            Debug.Assert(clips.Length == _pitches.Length * _velocities.Length);

            var idx = _pitches.AsSpan().BinarySearch(pitch);
            if (idx >= 0) {
                resultPitch = _pitches[idx];
                return clips.Slice(idx * 4, 4);
            }
            else {
                idx = ~idx;
                int resultIndex;
                if (idx == 0)
                    resultIndex = 0;
                else if (idx >= _pitches.Length)
                    resultIndex = _pitches.Length - 1;
                else if (_pitches[idx] - pitch < pitch - _pitches[idx - 1])
                    resultIndex = idx;
                else
                    resultIndex = idx - 1;
                resultPitch = _pitches[resultIndex];
                return clips.Slice(resultIndex * 4, 4);
            }
        }

        private static (int Pitch, (T? Clip, int Velocity) Low, (T Clip, int Velocity) High) GetNearestSoundClip<T>(int pitch, int velocity, ReadOnlySpan<T> soundClips)
        {
            var clips = GetNearestClipsByPitch(pitch, out var resultPitch, soundClips);
            Debug.Assert(clips.Length == _velocities.Length);

            for (int i = 0; i < _velocities.Length; i++) {
                if (velocity <= _velocities[i]) {
                    if (i > 0) {
                        return (resultPitch, (clips[i - 1], _velocities[i - 1]), (clips[i], _velocities[i]));
                    }
                    else {
                        return (resultPitch, default, (clips[i], _velocities[i]));
                    }
                }
            }
            return (resultPitch, default, default);
        }

        private static async UniTask PlayPianoSoundAsync(AudioSource source, int pitch, float? duration, float delay, float speed, bool[] clearPitchFlags)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay / speed));
            source.enabled = true;
            source.Play();

            if (duration is null) {
                await UniTask.WaitUntil(source, obj => !obj.isPlaying);
                source.enabled = false;
                return;
            }

            float timer = 0f;

            float fadeoutSpeed = source.volume * 0.015f;
            float clearSpeed = source.volume * 0.1f;
            float endVolume = source.volume * 0.01f;
            float actualDuration = duration.Value / speed;

            while (true) {
                await UniTask.Yield();
                timer += Time.deltaTime;
                if (timer >= actualDuration) {
                    if (clearPitchFlags[pitch]) {
                        fadeoutSpeed = clearSpeed;
                    }
                    source.volume -= fadeoutSpeed;
                    if (source.volume < endVolume) {
                        break;
                    }
                }
            }
            source.volume = 0f;
            source.Stop();

            source.enabled = false;
            return;
        }

        public static void PlayPianoSoundAsync(int pitch, int velocity, float? duration, float delay, float volume, float speed,
            ReadOnlySpan<AudioClip> clips, bool interpolateVelocity, bool[] clearPitchFlags, Func<AudioSource> getAudioSource, Action<AudioSource> returnAudioSource)
        {
            const int MaxPitch = 127;
            const float SemitoneRatio = 1.059463f; // 2^(1/12)

            velocity = Math.Min(velocity, MaxPitch);
            var (resultPitch, lowSound, highSound) = GetNearestSoundClip(pitch, velocity, clips);
            Debug.Assert(highSound.Clip != null);

            float sourcePitch = Mathf.Pow(SemitoneRatio, pitch - resultPitch);
            if (interpolateVelocity) {
                var playerHigh = getAudioSource();
                playerHigh.clip = highSound.Clip;
                playerHigh.pitch = sourcePitch * speed;
                var velDiff = highSound.Velocity - lowSound.Velocity;
                playerHigh.volume = (float)(velocity - lowSound.Velocity) / velDiff * volume;

                PlayPianoSoundAsync(playerHigh, pitch, duration, delay, speed, clearPitchFlags)
                    .ContinueWith((playerHigh, returnAudioSource), tpl => tpl.returnAudioSource(playerHigh))
                    .Forget();

                if (lowSound.Clip is null)
                    return;

                var playerLow = getAudioSource();
                playerLow.clip = lowSound.Clip;
                playerLow.pitch = sourcePitch * speed;
                playerLow.volume = (float)(highSound.Velocity - velocity) / velDiff * volume;

                PlayPianoSoundAsync(playerLow, pitch, duration, delay, speed, clearPitchFlags)
                    .ContinueWith((playerLow, returnAudioSource), tpl => tpl.returnAudioSource(playerLow))
                    .Forget();
            }
            else {
                var player = getAudioSource();
                player.clip = highSound.Clip;
                player.pitch = sourcePitch * speed;
                player.volume = (float)velocity / highSound.Velocity * volume;

                PlayPianoSoundAsync(player, pitch, duration, delay, speed, clearPitchFlags)
                    .ContinueWith((player, returnAudioSource), tpl => tpl.returnAudioSource(player))
                    .Forget();
            }

            clearPitchFlags[pitch] = true;
        }

        public static void LateUpdate_ClearPianoSoundClearPitchFlags(bool[] clearPitchFlags)
        {
            Array.Fill(clearPitchFlags, false);
        }

        public static bool[] CreatePianoSoundClearPitchFlags() => new bool[128];
    }
}