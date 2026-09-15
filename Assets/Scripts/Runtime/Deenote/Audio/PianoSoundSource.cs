using Cysharp.Threading.Tasks;
using Deenote.Library;
using Deenote.Replica;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Audio
{
    public sealed class PianoSoundSource : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioPlayerPrefab = null!;
        private ObjectPool<AudioSource> _audioPlayerPool = null!;

        [SerializeField] private AudioClip[] _soundClips = null!;
        [SerializeField] private bool _interpolateVelocity;

        private bool[] _clearPitch;

        public async UniTaskVoid PlaySoundAsync(int pitch, int velocity, float? duration, float delay, float speed, float volume = 1)
        {
            if (velocity == 0 || duration == 0)
                return;

            DeemoReplica.PlayPianoSoundAsync(pitch, velocity, duration, delay, volume, speed,
                _soundClips, _interpolateVelocity, _clearPitch, _audioPlayerPool.Get, _audioPlayerPool.Release);
            return;
        }

        private void Awake()
        {
            _audioPlayerPool = UnityUtils.CreateObjectPool(_audioPlayerPrefab, transform, defaultCapacity: 0);
            _clearPitch = DeemoReplica.CreatePianoSoundClearPitchFlags();
        }

        private void LateUpdate()
        {
            DeemoReplica.LateUpdate_ClearPianoSoundClearPitchFlags(_clearPitch);
        }
    }
}