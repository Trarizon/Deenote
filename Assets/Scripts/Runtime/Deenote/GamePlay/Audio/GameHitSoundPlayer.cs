#nullable enable

using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Models.Notes;
using UnityEngine;

namespace Deenote.GamePlay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    internal sealed class GameHitSoundPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource _source = default!;
        [SerializeField] HitSoundConfig _config = default!;

        public float Volume { get; internal set; }

        public void PlaySound(NoteKind kind)
        {
            if (Volume <= 0f)
                return;

            switch (kind) {
                case NoteKind.Slide:
                    _source.PlayOneShot(_config.SlideHitSoundClip, Volume * _config.SlideHitSoundBaseVolume);
                    break;
                case NoteKind.Click:
                case NoteKind.Swipe:
                    _source.PlayOneShot(_config.ClickHitSoundClip, Volume * _config.ClickHitSoundBaseVolume);
                    break;
            }
        }

        private void OnValidate()
        {
            _source ??= GetComponent<AudioSource>();
        }

        public enum SoundKind
        {
            ClickSound,
            SlideSound,
        }
    }
}