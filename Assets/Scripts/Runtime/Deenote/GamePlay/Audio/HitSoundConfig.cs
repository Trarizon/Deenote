#nullable enable

using UnityEngine;

namespace Deenote.Core.GamePlay.Audio
{
    [CreateAssetMenu(
        fileName =nameof(HitSoundConfig),
        menuName = $"Deenote/GamePlay/HitSoundConfig")]
    internal sealed class HitSoundConfig : ScriptableObject
    {
#nullable disable
        public AudioClip ClickHitSoundClip;
        public float ClickHitSoundBaseVolume = 1f;
        public AudioClip SlideHitSoundClip;
        public float SlideHitSoundBaseVolume = 1f;
    }
}
