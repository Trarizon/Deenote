#nullable enable

using System;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        private void RegisterConfigurations()
        {
            //MainSystem.SaveSystem.SavingConfigurations += configs =>
            //{
            //    configs.Set("stage/highlight_note_speed", HighlightedNoteSpeed);
            //    configs.Set("stage/apply_speed_diff", IsApplySpeedDifference);
            //    configs.Set("stage/filter_note_speed", IsFilterNoteSpeed);

            //    configs.Set("stage/note_speed", NoteFallSpeed);
            //    configs.Set("stage/show_link_lines", IsShowLinkLines);
            //    configs.Set("stage/piano_note_distinguish", IsPianoNotesDistinguished);
            //    configs.Set("stage/effect", IsStageEffectOn);
            //    configs.Set("stage/sudden_plus", SuddenPlus);
            //    configs.Set("stage/early_display_slow_notes", EarlyDisplaySlowNotes);
            //    configs.Set("stage/pause_when_lose_focus", PauseWhenLoseFocus);

            //    configs.Set("stage/music_speed", MusicSpeed);
            //    configs.Set("stage/hitsound_volume", HitSoundVolume);
            //    configs.Set("stage/music_volume", MusicVolume);
            //    configs.Set("stage/piano_volume", PianoVolume);
            //};
            //MainSystem.SaveSystem.LoadedConfigurations += configs =>
            //{
            //    HighlightedNoteSpeed = configs.GetSingle("stage/highlight_note_speed", 1f);
            //    IsApplySpeedDifference = configs.GetBoolean("stage/apply_speed_diff", true);
            //    IsFilterNoteSpeed = configs.GetBoolean("stage/filter_note_speed", false);

            //    NoteFallSpeed = configs.GetInt32("stage/note_speed", 40);
            //    IsShowLinkLines = configs.GetBoolean("stage/show_link_lines", true);
            //    IsPianoNotesDistinguished = configs.GetBoolean("stage/piano_note_distinguish", true);
            //    IsStageEffectOn = configs.GetBoolean("stage/effect", true);
            //    SuddenPlus = configs.GetSingle("stage/sudden_plus", 0f);
            //    EarlyDisplaySlowNotes = configs.GetBoolean("stage/early_display_slow_notes", false);
            //    PauseWhenLoseFocus = configs.GetBoolean("stage/pause_when_lose_focus", true);

            //    MusicSpeed = configs.GetInt32("stage/music_speed", 10);
            //    HitSoundVolume = configs.GetSingle("stage/hitsound_volume", 0f);
            //    MusicVolume = configs.GetSingle("stage/music_volume", 100f);
            //    PianoVolume = configs.GetSingle("stage/piano_volume", 0f);
            //};
        }

        private const float ZeroAvoidHighlightedSpeed = 0.1f;


        /// <summary>
        /// If <see cref="IsFilterNoteSpeed"/> is <see langword="true"/>,
        /// a downplayed note will not be selectable on stage.
        /// <br/>
        /// The value is also the default value when place note by editor
        /// </summary>
        [Obsolete]
        public float HighlightedNoteSpeed
        {
            get => _stageContext.HighlightedNoteSpeed;
            set {
                _stageContext.HighlightedNoteSpeed = value;
                NotifyFlag(NotificationFlag.HighlightedNoteSpeed);
            }
        }

        [Obsolete("剩余引用应该可以直接删")]
        public bool IsApplySpeedDifference
        {
            get => _stageContext.IsApplySpeedDifference;
            set {
                _stageContext.IsApplySpeedDifference = value;
                NotifyFlag(NotificationFlag.IsApplySpeedDifference);
            }
        }

        [Obsolete()]
        public bool IsFilterNoteSpeed
        {
            get => _stageContext.IsFilterNoteSpeed;
            set {
                _stageContext.IsApplySpeedDifference = value;
                NotifyFlag(NotificationFlag.IsFilterNoteSpeed);
            }
        }

        #region Stage

        public const int MinNoteSpeed = 5;
        public const int MaxNoteSpeed = 95;

        /// <summary>
        /// Range [5, 95], display [0.5, 9.5]
        /// </summary>
        [Obsolete]
        public int NoteFallSpeed
        {
            get => _stageContext.NoteFallSpeed;
            set {
                _stageContext.NoteFallSpeed = value;
                NotifyFlag(NotificationFlag.NoteSpeed);
            }
        }

        [Obsolete]
        public bool IsShowLinkLines
        {
            get => _stageContext.IsShowLinkLines;
            set {
                _stageContext.IsShowLinkLines = value;
                NotifyFlag(NotificationFlag.IsShowLinkLines);
            }
        }

        [Obsolete]
        public bool IsPianoNotesDistinguished
        {
            get => _stageContext.IsPianoNotesDistinguished;
            set {
                _stageContext.IsPianoNotesDistinguished = value;
                NotifyFlag(NotificationFlag.DistinguishPianoNotes);
            }
        }

        [Obsolete]
        public bool IsStageEffectOn
        {
            get => _stageContext.IsStageEffectOn;
            set {
                _stageContext.IsStageEffectOn = value;
                NotifyFlag(NotificationFlag.StageEffectOn);
            }
        }

        /// <summary>
        /// Range [0, 1]
        /// </summary>
        [Obsolete]
        public float SuddenPlus
        {
            get => _stageContext.SuddenPlus;
            set {
                _stageContext.SuddenPlus = value;
                NotifyFlag(NotificationFlag.SuddenPlus);
            }
        }

        /// <remarks>
        /// In DEEMO II, if a low-speed notes is following a high-speed note, 
        /// the slow note will appear only when the fast one appeared.
        /// That means, the slow note will appear from the center of note panel.
        /// <br/>
        /// 
        /// </remarks>
        [Obsolete]
        public bool EarlyDisplaySlowNotes
        {
            get => _stageContext.IsEarlyDisplaySlowNotes;
            set {
                _stageContext.IsEarlyDisplaySlowNotes = value;
                NotifyFlag(NotificationFlag.EarlyDisplaySlowNotes);
            }
        }

        [Obsolete]
        public bool PauseWhenLoseFocus
        {
            get => _context.PauseWhenLoseFocus;
            set {
                _context.PauseWhenLoseFocus = value;
                NotifyFlag(NotificationFlag.PauseWhenLoseFocus);
            }
        }

        #endregion

        #region Audio

        public const int MinMusicSpeed = 1;
        public const int MaxMusicSpeed = 30;

        /// <summary>
        /// Range [1, 30], representing [0.1, 3.0]
        /// </summary>
        [Obsolete]
        public int MusicSpeed
        {
            get => _context.MusicSpeed;
            set {
                _context.MusicSpeed = value;
                var actualVal = ConvertToActualMusicSpeed(_context.MusicSpeed);
                MusicPlayer.Pitch = actualVal;
                PianoSoundPlayer.Speed = actualVal;
                NotifyFlag(NotificationFlag.MusicSpeed);
            }
        }

        /// <summary>
        /// Range [0,1]
        /// </summary>
        [Obsolete]
        public float HitSoundVolume
        {
            get => _context.HitSoundVolume;
            set {
                _context.HitSoundVolume = value;
                HitSoundPlayer.Volume = value;
                NotifyFlag(NotificationFlag.HitSoundVolume);
            }
        }

        /// <summary>
        /// Range [0,1]
        /// </summary>
        [Obsolete]
        public float MusicVolume
        {
            get => _context.MusicVolume;
            set {
                _context.MusicVolume = value;
                MusicPlayer.Volume = value;
                NotifyFlag(NotificationFlag.MusicVolume);
            }
        }

        /// <summary>
        /// Range [0,1]
        /// </summary>
        [Obsolete]
        public float PianoVolume
        {
            get => _context.PianoVolume;
            set {
                _context.PianoVolume = value;
                PianoSoundPlayer.Volume = value;
                NotifyFlag(NotificationFlag.PianoVolume);
            }
        }

        #endregion

        private static float ConvertToActualNoteSpeed(int noteSpeed) => noteSpeed / 10f;
        private static float ConvertToActualMusicSpeed(int musicSpeed) => musicSpeed / 10f;
    }
}