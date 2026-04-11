#nullable enable

using Deenote.Contexts;
using Deenote.Core;
using Deenote.CoreB.Notification;
using Deenote.Library;
using System;

namespace Deenote.GamePlay
{
    public sealed class GamePlayContext : INotifyPropertyChanged<GamePlayContext>
    {
        private readonly ProjectContext _project;

        private float _currentTime_bf;
        public float CurrentTime
        {
            get => _currentTime_bf;
            set {
                if (Utils.SetField(ref _currentTime_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(CurrentTime)));
                }
            }
        }

        #region Properties

        private bool _pauseWhenLoseFocus;
        public bool PauseWhenLoseFocus
        {
            get => _pauseWhenLoseFocus;
            set {
                if (Utils.SetField(ref _pauseWhenLoseFocus, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(PauseWhenLoseFocus)));
                }
            }
        }

        private const int MinMusicSpeed = 1;
        private const int MaxMusicSpeed = 30;
        private int _musicSpeed_bf;
        /// <summary>
        /// Range [1, 30], representing [0.1, 3.0]
        /// </summary>
        public int MusicSpeed
        {
            get => _musicSpeed_bf;
            set {
                value = Math.Clamp(value, MinMusicSpeed, MaxMusicSpeed);
                if (Utils.SetField(ref _musicSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(MusicSpeed)));
                    PropertyChanged?.Invoke(this, new(nameof(ActualMusicSpeed)));
                }
            }
        }

        public float ActualMusicSpeed => ConvertToActualMusicSpeed(MusicSpeed);

        private float _hitSoundVolume_bf;
        /// <summary>
        /// Range [0,1]
        /// </summary>
        public float HitSoundVolume
        {
            get => _hitSoundVolume_bf;
            set {
                value = Math.Clamp(value, 0f, 1f);
                if (Utils.SetField(ref _hitSoundVolume_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(HitSoundVolume)));
                }
            }
        }

        private float _musicVolume_bf;
        /// <summary>
        /// Range [0,1]
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume_bf;
            set {
                value = Math.Clamp(value, 0f, 1f);
                if (Utils.SetField(ref _musicVolume_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(MusicVolume)));
                }
            }
        }

        private float _pianoVolume_bf;
        /// <summary>
        /// Range [0,1]
        /// </summary>
        public float PianoVolume
        {
            get => _pianoVolume_bf;
            set {
                value = Math.Clamp(value, 0f, 1f);
                if (Utils.SetField(ref _pianoVolume_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(PianoVolume)));
                }
            }
        }

        #endregion

        public event Action<GamePlayContext, PropertyEventArgs>? PropertyChanged;

        public GamePlayContext(ProjectContext project, SaveSystem storage)
        {
            _project = project;

            storage.SavingConfigurations += configs =>
            {
                configs.Set("stage/pause_when_lose_focus", PauseWhenLoseFocus);

                configs.Set("stage/music_speed", MusicSpeed);
                configs.Set("stage/hitsound_volume", HitSoundVolume);
                configs.Set("stage/music_volume", MusicVolume);
                configs.Set("stage/piano_volume", PianoVolume);
            };
            storage.LoadedConfigurations += configs =>
            {
                PauseWhenLoseFocus = configs.GetBoolean("stage/pause_when_lose_focus", true);

                MusicSpeed = configs.GetInt32("stage/music_speed", 10);
                HitSoundVolume = configs.GetSingle("stage/hitsound_volume", 0f);
                MusicVolume = configs.GetSingle("stage/music_volume", 100f);
                PianoVolume = configs.GetSingle("stage/piano_volume", 0f);
            };
        }

        private static float ConvertToActualMusicSpeed(int musicSpeed) => musicSpeed / 10f;
    }
}
