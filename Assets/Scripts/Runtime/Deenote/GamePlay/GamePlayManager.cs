using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Notification;
using Deenote.GamePlay.Audio;
using UnityEngine;

namespace Deenote.GamePlay
{
    public sealed partial class GamePlayManager2 : INotifyPropertyChanged<GamePlayManager2>
    {
        private readonly GameMusicPlayer _musicPlayer;
        private readonly GameNoteSoundsPlayer _noteSoundsPlayer;

        public float CurrentTime => _musicPlayer.Time;

        public AudioClip? CurrentAudioClip => App.ProjectManager.CurrentAudioClip;

        public GamePlayManager2(GameMusicPlayer musicPlayer, PianoSoundSource pianoSoundSource, HitSoundSource hitSoundSource)
        {
            _musicPlayer = musicPlayer;
            _noteSoundsPlayer = new GameNoteSoundsPlayer(pianoSoundSource, hitSoundSource);

            App.Current.UnscaledTick += delta =>
            {
                if (App.ProjectManager.CurrentChart is null)
                    return;

                Update_ManualPlay(delta);
                _noteSoundsPlayer.UpdateTime(CurrentTime);
            };

            App.Current.FocusChanged += focus =>
            {
                if (PauseWhenLoseFocus && !focus) {
                    _musicPlayer.Stop();
                }
            };
        }
    }
}
