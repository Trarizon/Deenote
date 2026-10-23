using System;
using TriInspector;
using UnityEngine;

namespace Deenote.GamePlay.Audio
{
    public class HitSoundSource : MonoBehaviour
    {
        [Required][SerializeField] AudioSource _source;

        [SerializeField] SoundItem _clickSound;
        [SerializeField] SoundItem _slideSound;

        [Serializable]
        private struct SoundItem
        {
            public AudioClip Clip;
            [Min(0)] public float BaseVolume;
        }

        public void PlaySound(SoundKind kind, float volume)
        {
            switch (kind) {
                case SoundKind.Click:
                    _source.PlayOneShot(_clickSound.Clip, volume * _clickSound.BaseVolume);
                    break;
                case SoundKind.Slide:
                    _source.PlayOneShot(_slideSound.Clip, volume * _slideSound.BaseVolume);
                    break;
            }
        }

        public enum SoundKind { Click, Slide, }
    }
}