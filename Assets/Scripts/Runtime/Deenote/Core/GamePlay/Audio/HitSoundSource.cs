#nullable enable

using Deenote.Entities.Models;
using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Deenote.Audio
{
    public sealed class HitSoundSource : MonoBehaviour
    {
        private const double MinScheduleLeadSeconds = 0.01d;
        private const double VoiceRecycleSlackSeconds = 0.01d;
        private const double ActiveStartGraceSeconds = 0.01d;

        [Serializable]
        private struct SoundItem
        {
            public AudioClip Clip;
            [Min(0)] public float BaseVolume;
        }

        [Required][SerializeField] private AudioSource _source;
        [SerializeField] SoundItem _clickSound;
        [SerializeField] SoundItem _slideSound;

        [SerializeField, Min(1)] private int _maxVoices = 16;

        private readonly List<Voice> _voices = new();

        private enum VoiceState
        {
            Free,
            Scheduled,
            Active,
        }

        private void Awake()
        {
            _voices.Add(CreateVoice(_source));
            for (int i = 1; i < _maxVoices; i++)
                _voices.Add(CreateVoice(null));
        }

        private void Update()
        {
            double dspTime = AudioSettings.dspTime;
            for (int i = 0; i < _voices.Count; i++) {
                var voice = _voices[i];
                if (voice.State == VoiceState.Free)
                    continue;

                if (voice.State == VoiceState.Scheduled &&
                    dspTime + ActiveStartGraceSeconds >= voice.StartDsp) {
                    voice.State = VoiceState.Active;
                }

                if (voice.State == VoiceState.Scheduled)
                    continue;
                if (dspTime < voice.EndDsp + VoiceRecycleSlackSeconds)
                    continue;

                voice.Source.Stop();
                voice.State = VoiceState.Free;
            }
        }

        public void PlaySound(SoundKind kind, float volume)
            => ScheduleSound(kind, AudioSettings.dspTime + MinScheduleLeadSeconds, volume);

        public bool ScheduleSound(SoundKind kind, double targetDspTime, float volume)
        {
            if (volume <= 0f)
                return false;
            if (targetDspTime < AudioSettings.dspTime + MinScheduleLeadSeconds)
                return false;

            AudioClip clip;
            float baseVolume;
            switch (kind) {
                case SoundKind.Slide:
                    clip = _slideSound.Clip;
                    baseVolume = _slideSound.BaseVolume;
                    break;
                case SoundKind.Click:
                    clip = _clickSound.Clip;
                    baseVolume = _clickSound.BaseVolume;
                    break;
                default:
                    return false;
            }

            if (clip is null)
                return false;

            var voice = AcquireVoice();
            var source = voice.Source;
            source.Stop();
            source.clip = clip;
            source.loop = false;
            source.volume = volume * baseVolume;
            source.pitch = 1f;
            source.PlayScheduled(targetDspTime);

            double endDsp = targetDspTime + clip.length;
            source.SetScheduledEndTime(endDsp);

            voice.EndDsp = endDsp;
            voice.StartDsp = targetDspTime;
            voice.State = VoiceState.Scheduled;
            return true;
        }

        public void CancelScheduledSounds()
        {
            for (int i = 0; i < _voices.Count; i++) {
                var voice = _voices[i];
                if (voice.State == VoiceState.Free)
                    continue;

                voice.Source.Stop();
                voice.State = VoiceState.Free;
            }
        }

        public void CancelPendingSounds()
        {
            double dspTime = AudioSettings.dspTime;
            for (int i = 0; i < _voices.Count; i++) {
                var voice = _voices[i];
                if (voice.State == VoiceState.Scheduled &&
                    dspTime + ActiveStartGraceSeconds >= voice.StartDsp) {
                    voice.State = VoiceState.Active;
                }

                if (voice.State != VoiceState.Scheduled)
                    continue;

                voice.Source.Stop();
                voice.State = VoiceState.Free;
            }
        }

        private Voice AcquireVoice()
        {
            Voice? oldest = null;
            for (int i = 0; i < _voices.Count; i++) {
                var voice = _voices[i];
                if (voice.State == VoiceState.Free)
                    return voice;
                if (oldest is null || voice.EndDsp < oldest.EndDsp)
                    oldest = voice;
            }

            // Voice stealing is preferable to delaying a scheduled sound.
            Debug.Assert(oldest is not null);
            return oldest!;
        }

        private Voice CreateVoice(AudioSource? template)
        {
            if (template is null) {
                var go = new GameObject("HitSoundVoice");
                go.transform.SetParent(transform, false);
                template = go.AddComponent<AudioSource>();
            }

            template.playOnAwake = false;
            template.loop = false;

            var voice = new Voice(template);
            if (template != _source) {
                var source = _source;
                template.outputAudioMixerGroup = source.outputAudioMixerGroup;
                template.spatialBlend = source.spatialBlend;
                template.rolloffMode = source.rolloffMode;
                template.minDistance = source.minDistance;
                template.maxDistance = source.maxDistance;
                template.priority = source.priority;
                template.bypassEffects = source.bypassEffects;
                template.bypassListenerEffects = source.bypassListenerEffects;
                template.bypassReverbZones = source.bypassReverbZones;
                template.dopplerLevel = source.dopplerLevel;
                template.spread = source.spread;
                template.panStereo = source.panStereo;
                template.reverbZoneMix = source.reverbZoneMix;
                template.mute = source.mute;
            }

            return voice;
        }

        private void OnValidate()
        {
            _source ??= GetComponent<AudioSource>();
            _maxVoices = Mathf.Max(1, _maxVoices);
        }

        private sealed class Voice
        {
            public readonly AudioSource Source;
            public double StartDsp;
            public double EndDsp;
            public VoiceState State;

            public Voice(AudioSource source)
            {
                Source = source;
            }
        }

        public enum SoundKind { Click, Slide, }
    }
}
