using Cysharp.Threading.Tasks;
using Deenote.Library;
using Deenote.Replica;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Audio
{
    public sealed class PianoSoundSource : MonoBehaviour
    {
        public const int MaxPitch = 127;
        public const int MinPitch = 0;

        private const double ActiveStartGraceSeconds = 0.01d;

        [SerializeField] private AudioSource _audioPlayerPrefab = null!;
        private ObjectPool<AudioSource> _audioPlayerPool = null!;

        [SerializeField] private AudioClip[] _soundClips = null!;
        [SerializeField] private bool _interpolateVelocity;

        private bool[] _clearPitch;
        private readonly List<ScheduledVoice> _scheduledVoices = new(32);

        // Mirrored from Chlorie's
        public async UniTaskVoid PlaySoundAsync(int pitch, int velocity, float? duration, float delay, float speed, float volume = 1)
        {
            if (velocity == 0 || duration == 0)
                return;

            DeemoReplica.PlayPianoSoundAsync(pitch, velocity, duration, delay, volume, speed,
                _soundClips, _interpolateVelocity, _clearPitch, _audioPlayerPool.Get, _audioPlayerPool.Release);
            return;
        }

        /// <summary>
        /// Schedules a note sample at an absolute DSP time. Unlike the immediate
        /// path above, this method does not use a coroutine or frame-based delay.
        /// </summary>
        public void ScheduleSound(int pitch, int velocity, float? duration, float speed,
            double targetDspTime, float volume = 1f)
        {
            if (velocity == 0 || duration == 0)
                return;
            if (targetDspTime < AudioSettings.dspTime + 0.01d)
                return;

            velocity = Math.Min(velocity, 127);
            var (resultPitch, lowSound, highSound) = DeemoReplica.GetNearestSoundClip<AudioClip>(pitch, velocity, _soundClips);
            if (highSound.Clip is null)
                return;

            float sourcePitch = Mathf.Pow(1.059463f, pitch - resultPitch);
            if (_interpolateVelocity) {
                float velDiff = highSound.Velocity - lowSound.Velocity;
                if (velDiff <= 0f)
                    return;

                ScheduleVoice(highSound.Clip, sourcePitch * speed,
                    (velocity - lowSound.Velocity) / velDiff * volume,
                    duration, speed, targetDspTime);

                if (lowSound.Clip is not null) {
                    ScheduleVoice(lowSound.Clip, sourcePitch * speed,
                        (highSound.Velocity - velocity) / velDiff * volume,
                        duration, speed, targetDspTime);
                }
            }
            else {
                ScheduleVoice(highSound.Clip, sourcePitch * speed,
                    (float)velocity / highSound.Velocity * volume,
                    duration, speed, targetDspTime);
            }
        }

        private void ScheduleVoice(AudioClip clip, float pitch, float volume,
            float? duration, float speed, double targetDspTime)
        {
            var source = _audioPlayerPool.Get();
            source.enabled = true;
            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;
            source.loop = false;
            source.PlayScheduled(targetDspTime);

            float safeSpeed = Mathf.Max(0.01f, speed);
            float naturalDuration = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
            float requestedDuration = duration is { } durationValue && durationValue > 0f
                ? durationValue / safeSpeed
                : naturalDuration;

            double endDspTime = targetDspTime +
                Math.Min(requestedDuration, naturalDuration);
            source.SetScheduledEndTime(endDspTime);
            _scheduledVoices.Add(new ScheduledVoice(
                source,
                targetDspTime,
                endDspTime));
        }

        public void CancelScheduledSounds()
        {
            for (int i = _scheduledVoices.Count - 1; i >= 0; i--) {
                var voice = _scheduledVoices[i];
                voice.Source.Stop();
                voice.Source.enabled = false;
                _audioPlayerPool.Release(voice.Source);
                _scheduledVoices.RemoveAt(i);
            }
        }

        public void CancelPendingSounds()
        {
            double dspTime = AudioSettings.dspTime;
            for (int i = _scheduledVoices.Count - 1; i >= 0; i--) {
                var voice = _scheduledVoices[i];
                if (voice.State == ScheduledVoiceState.Scheduled &&
                    dspTime + ActiveStartGraceSeconds >= voice.StartDspTime) {
                    voice.State = ScheduledVoiceState.Active;
                }

                if (voice.State != ScheduledVoiceState.Scheduled)
                    continue;

                voice.Source.Stop();
                voice.Source.enabled = false;
                _audioPlayerPool.Release(voice.Source);
                _scheduledVoices.RemoveAt(i);
            }
        }

        private void Awake()
        {
            // Warm the pool before the first play so scheduling the first
            // notes does not instantiate AudioSources on the input frame.
            _audioPlayerPool = UnityUtils.CreateObjectPool(_audioPlayerPrefab, transform, defaultCapacity: 16);
            _clearPitch = DeemoReplica.CreatePianoSoundClearPitchFlags();
        }

        private void LateUpdate()
        {
            double dspTime = AudioSettings.dspTime;
            for (int i = _scheduledVoices.Count - 1; i >= 0; i--) {
                var voice = _scheduledVoices[i];
                if (voice.State == ScheduledVoiceState.Scheduled &&
                    dspTime + ActiveStartGraceSeconds >= voice.StartDspTime) {
                    voice.State = ScheduledVoiceState.Active;
                }

                if (dspTime < voice.EndDspTime + 0.01d)
                    continue;

                voice.Source.Stop();
                voice.Source.enabled = false;
                _audioPlayerPool.Release(voice.Source);
                _scheduledVoices.RemoveAt(i);
            }

            DeemoReplica.LateUpdate_ClearPianoSoundClearPitchFlags(_clearPitch);
        }

        private enum ScheduledVoiceState
        {
            Scheduled,
            Active,
        }

        private sealed class ScheduledVoice
        {
            public readonly AudioSource Source;
            public readonly double StartDspTime;
            public readonly double EndDspTime;
            public ScheduledVoiceState State;

            public ScheduledVoice(AudioSource source, double startDspTime,
                double endDspTime)
            {
                Source = source;
                StartDspTime = startDspTime;
                EndDspTime = endDspTime;
                State = ScheduledVoiceState.Scheduled;
            }
        }
    }
}
