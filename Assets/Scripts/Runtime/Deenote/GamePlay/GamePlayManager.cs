using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Notification;
using System;
using UnityEngine;

namespace Deenote.GamePlay
{
    public sealed partial class GamePlayManager2 : INotifyPropertyChanged<GamePlayManager2>
    {
        private readonly GameMusicPlayer _musicPlayer;
        private readonly GameNoteSoundsPlayer _noteSoundsPlayer;

        // TODO: r1.1 音频重构时note音频播放需要这个，不知道为什么，如果后续不需要的话可以不公开
        internal GameMusicPlayer MusicPlayer => _musicPlayer;

        // TODO: r1.1 临时公开给旧GamePlayManager使用
        [Obsolete("临时公开给旧GamePlayManager使用")]
        internal GameNoteSoundsPlayer NoteSoundsPlayer => _noteSoundsPlayer;

        public float CurrentTime => _musicPlayer.Time;

        public AudioClip? CurrentAudioClip => App.ProjectManager.CurrentAudioClip;

        public GamePlayManager2(GameMusicPlayer musicPlayer, PianoSoundSource pianoSoundSource, HitSoundSource hitSoundSource)
        {
            _musicPlayer = musicPlayer;
            _noteSoundsPlayer = new GameNoteSoundsPlayer(musicPlayer, pianoSoundSource, hitSoundSource);

            App.Current.UnscaledTick += delta =>
            {
                if (App.ProjectManager.CurrentChart is null)
                    return;

                Update_ManualPlay(delta);
                _noteSoundsPlayer.ScheduleAudio();
            };

            App.Current.FocusChanged += focus =>
            {
                if (PauseWhenLoseFocus && !focus) {
                    _musicPlayer.Stop();
                }
            };

            _musicPlayer.TransportChanged += reason =>
            {
                if (App.ProjectManager.CurrentChart is null)
                    return;

                switch (reason) {
                    case TransportChangeReason.Paused:
                        // Let already-started voices ring out, but discard
                        // everything that has only been scheduled for the future.
                        _noteSoundsPlayer.CancelPendingAudio();
                        return;
                    case TransportChangeReason.Seeked:
                        // Time jumps only invalidate scheduled voices. Active
                        // voices continue as tails.
                        _noteSoundsPlayer.CancelPendingAudio();
                        break;
                    case TransportChangeReason.RateChanged:
                        _noteSoundsPlayer.CancelPendingAudio();
                        break;
                    case TransportChangeReason.Started:
                        _noteSoundsPlayer.RebaseScheduledAudio();
                        break;
                }

                _noteSoundsPlayer.ScheduleAudio();
            };

            App.ProjectManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    _noteSoundsPlayer.Chart = s.CurrentChart;
                }
            });
        }
    }
}
