using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Notification;
using System;
using UnityEngine;

namespace Deenote.GamePlay
{
    public sealed partial class GamePlayerManager2 : INotifyPropertyChanged<GamePlayerManager2>
    {
        private readonly GameMusicPlayer _musicPlayer;
        private readonly StagePianoSoundPlayer _pianoSoundPlayer;
        private readonly HitSoundPlayer _hitSoundPlayer;

        public float CurrentTime => _musicPlayer.Time;

        public AudioClip? CurrentAudioClip => App.ProjectManager.CurrentAudioClip;

        internal StagePianoSoundPlayer PianoSoundPlayer => _pianoSoundPlayer;

        public GamePlayerManager2(GameMusicPlayer musicPlayer, StagePianoSoundPlayer pianoSoundPlayer, HitSoundPlayer hitSoundPlayer)
        {
            _musicPlayer = musicPlayer;
            _pianoSoundPlayer = pianoSoundPlayer;
            _hitSoundPlayer = hitSoundPlayer;

            App.Current.UnscaledTick += delta =>
            {
                if (App.ProjectManager.CurrentChart is null)
                    return;

                Update_ManualPlay(delta);
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
