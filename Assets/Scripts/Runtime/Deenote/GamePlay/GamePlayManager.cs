using Cysharp.Threading.Tasks;
using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.CoreB.Helpers;
using Deenote.CoreB.Notification;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Deenote.GamePlay
{
    public sealed partial class GamePlayManager2 : INotifyPropertyChanged<GamePlayManager2>
    {
        private readonly GameMusicPlayer _musicPlayer;
        private readonly GameNoteSoundsPlayer _noteSoundsPlayer;

        // TODO: r1.1 临时公开给旧GamePlayManager使用
        [Obsolete("临时公开给旧GamePlayManager使用")]
        internal GameNoteSoundsPlayer NoteSoundsPlayer => _noteSoundsPlayer;

        public float CurrentTime => _musicPlayer.Time;

        private AudioClip? _currentAudioClip;
        public AudioClip? CurrentAudioClip
        {
            get => _currentAudioClip;
            set {
                if (_currentAudioClip != value) {
                    _currentAudioClip = value;
                    PropertyChanged?.Invoke(this, nameof(CurrentAudioClip), nameof(GamePlayLength));
                }
            }
        }

        // 如果音乐未加载，取最后一个note的长度
        public float GamePlayLength => CurrentAudioClip?.length ?? 0;

        internal event Action<GameMusicPlayer.TimeChangedEventArgs> TimeChanged
        {
            add => _musicPlayer.TimeChanged += value;
            remove => _musicPlayer.TimeChanged -= value;
        }

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

            App.ProjectManager.RegisterPropertyChangedAndInvoke((s, e, t) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    _noteSoundsPlayer.Chart = s.CurrentChart;
                    if (s.CurrentChart is null) {
                        _musicPlayer.Stop();
                    }
                }
                s.CurrentProject2.AsNestedRegisterPropertyChangedAndInvoke(nameof(s.CurrentProject2), e, t, (s, e, t) =>
                {
                    if (s is null) {
                        CurrentAudioClip = null;
                        return;
                    }
                    if (e.Match(nameof(s.AudioFileData))) {
                        if (s.DecodedClip is not null) {
                            CurrentAudioClip = s.DecodedClip;
                        }
                        else {
                            LoadAudioClipFromDataAsync(s.AudioFileData, Path.GetExtension(s.AudioFileRelativePath)).Forget();
                        }
                    }
                });
            });
        }

        public void PostConstructor()
        {
            RegisterConfigurations();
        }

        private CancellationTokenSource? _audioClipCts;
        private async UniTask LoadAudioClipFromDataAsync(byte[] bytes, string audioType)
        {
            using var ms = new MemoryStream(bytes);

            if (_audioClipCts is not null) {
                _audioClipCts?.Cancel();
                _audioClipCts?.Dispose();
                _audioClipCts = null;
            }
            _audioClipCts = new CancellationTokenSource();

            var clip = await AudioUtils.TryLoadAsync(ms, audioType, _audioClipCts.Token);
            if (clip is null) {
                _musicPlayer.ReplaceClip(null);
                CurrentAudioClip = null;
                return;
            }

            // TODO: try out streaming clip provider
            _musicPlayer.ReplaceClip(new DecodedClipProvider(clip));
            CurrentAudioClip = clip;
        }
    }
}
