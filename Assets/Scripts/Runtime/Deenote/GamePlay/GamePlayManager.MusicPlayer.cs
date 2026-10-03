using Deenote.CoreB.Notification;

namespace Deenote.GamePlay
{
    partial class GamePlayerManager2 : INotifyPropertyChanged<GamePlayerManager2>
    {
        /// <remarks>
        /// If music is paused, maually set music time by this value,
        /// <br />
        /// If playing, this value should sync to _musicSource.velocity
        /// </remarks>
        private float? _manualPlaySpeedMultiplier;

        public float StagePlaySpeed => _manualPlaySpeedMultiplier ?? _musicPlayer.Pitch;

        public void SetManualPlaySpeed(float? manualPlaySpeed)
        {
            if (manualPlaySpeed is { } speed) {
                if (speed == 0) {
                    _musicPlayer.Pitch = ActualMusicSpeed;
                    _musicPlayer.Stop();
                }
                else {
                    _musicPlayer.Pitch = speed;
                }
                _manualPlaySpeedMultiplier = speed;
            }
            else {
                _musicPlayer.Pitch = ActualMusicSpeed;
                _manualPlaySpeedMultiplier = null;
            }
        }

        private void Update_ManualPlay(float deltaTime)
        {
            if (!_musicPlayer.IsPlaying && _manualPlaySpeedMultiplier is { } mSpeed) {
                _musicPlayer.Nudge(deltaTime * mSpeed);
            }
        }

        public void TogglePlayingState()
        {
            if (_musicPlayer.IsPlaying) {
                _musicPlayer.Stop();
            }
            else {
                _musicPlayer.Play();
            }
        }
    }
}
