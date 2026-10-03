using Deenote.CoreB.Notification;
using System;
using UnityEngine;

namespace Deenote.GamePlay
{
    partial class GamePlayerManager2 : INotifyPropertyChanged<GamePlayerManager2>
    {
        public event Action<GamePlayerManager2, PropertyChangedEventArgs>? PropertyChanged;

        public const int MinMusicSpeed = 1;
        public const int MaxMusicSpeed = 30;

        private int _musicSpeed_bf;
        /// <summary>
        /// Range [1, 30], representing [0.1, 3.0]
        /// </summary>
        public int MusicSpeed
        {
            get => _musicSpeed_bf;
            set {
                value = Mathf.Clamp(value, MinMusicSpeed, MaxMusicSpeed);
                if (_musicSpeed_bf != value) {
                    _musicSpeed_bf = value;

                    _musicPlayer.Pitch = ConvertToActualMusicSpeed(value);
                    PropertyChanged?.Invoke(this, nameof(MusicSpeed), nameof(ActualMusicSpeed));
                }
            }
        }

        public float ActualMusicSpeed => MusicSpeed / 10.0f;

        private float _musicVolume_bf;
        public float MusicVolume
        {
            get => _musicVolume_bf;
            set {
                value = Mathf.Clamp01(value);
                if (_musicVolume_bf != value) {
                    _musicVolume_bf = value;

                    _musicPlayer.Volume = value;
                    PropertyChanged?.Invoke(this, nameof(MusicVolume));
                }
            }
        }

        private float _pianoVolume_bf;
        public float PianoVolume
        {
            get => _pianoVolume_bf;
            set {
                value = Mathf.Clamp01(value);
                if (_pianoVolume_bf != value) {
                    _pianoVolume_bf = value;

                    _pianoSoundPlayer.Volume = value;
                    PropertyChanged?.Invoke(this, nameof(PianoVolume));
                }
            }
        }

        private float _hitSoundVolume_bf;
        public float HitSoundVolume
        {
            get => _hitSoundVolume_bf;
            set {
                value = Mathf.Clamp01(value);
                if (_hitSoundVolume_bf != value) {
                    _hitSoundVolume_bf = value;

                    _hitSoundPlayer.Volume = value;
                    PropertyChanged?.Invoke(this, nameof(HitSoundVolume));
                }
            }
        }

        private static float ConvertToActualMusicSpeed(int musicSpeed) => musicSpeed / 10.0f;
    
        private bool _pauseWhenLoseFocus_bf;
        public bool PauseWhenLoseFocus
        {
            get => _pauseWhenLoseFocus_bf;
            set {
                if (_pauseWhenLoseFocus_bf != value) {
                    _pauseWhenLoseFocus_bf = value;
                    PropertyChanged?.Invoke(this, nameof(PauseWhenLoseFocus));
                }
            }
        }
    }
}
