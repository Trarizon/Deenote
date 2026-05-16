#nullable enable

using Deenote.Contexts;
using Deenote.Core;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.GameStage.Themes;
using Deenote.Library;
using System;
using UnityEngine;
using Deenote.Library.Mathematics;
using Deenote.GamePlay;
using Deenote.GameStage.Stage;
using Deenote.GameStage.UI;

namespace Deenote.GameStage
{
    public sealed class GameStageContext : INotifyPropertyChanged<GameStageContext>
    {
        public ProjectContext ProjectContext { get; }
        public GameStageThemeContext ThemeContext { get; }
        internal GameStageNotesContext NotesContext { get; }

        public IPerspectiveViewPanelInfoProvider PerspectiveViewPanelInfo { get; }

        public GameStageController? GameStage => ThemeContext.CurrentTheme?.Stage;

        #region Properties

        private const int MinNoteSpeed = 5;
        private const int MaxNoteSpeed = 95;

        private int _noteSpeed_bf;
        /// <summary>
        /// Range [5, 95], display [0.5, 9.5]
        /// </summary>
        public int NoteFallSpeed
        {
            get => _noteSpeed_bf;
            set {
                value = Math.Clamp(value, MinNoteSpeed, MaxNoteSpeed);
                if (Utils.SetField(ref _noteSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(NoteFallSpeed)));
                    PropertyChanged?.Invoke(this, new(nameof(ActualNoteFallSpeed)));
                }
            }
        }

        public float ActualNoteFallSpeed => ConvertToActualNoteSpeed(NoteFallSpeed);

        private bool _showLinks_bf;
        public bool IsShowLinkLines
        {
            get => _showLinks_bf;
            set {
                if (Utils.SetField(ref _showLinks_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsShowLinkLines)));
                }
            }
        }

        private bool _distinguishPianoNotes_bf;
        public bool IsPianoNotesDistinguished
        {
            get => _distinguishPianoNotes_bf;
            set {
                if (Utils.SetField(ref _distinguishPianoNotes_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsPianoNotesDistinguished)));
                }
            }
        }

        private bool _stageEffect_bf;
        public bool IsStageEffectOn
        {
            get => _stageEffect_bf;
            set {
                if (Utils.SetField(ref _stageEffect_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsStageEffectOn)));
                }
            }
        }

        private float _suddenPlus_bf;
        /// <summary>
        /// Range [0, 1]
        /// </summary>
        public float SuddenPlus
        {
            get => _suddenPlus_bf;
            set {
                value = Math.Clamp(value, 0f, 1f);
                if (Utils.SetField(ref _suddenPlus_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(SuddenPlus)));
                }
            }
        }

        private bool earlyDisplaySlowNotes_bf;
        /// <remarks>
        /// In DEEMO II, if a low-speed notes is following a high-speed note, 
        /// the slow note will appear only when the fast one appeared.
        /// That means, the slow note will appear from the center of note panel.
        /// </remarks>
        public bool IsEarlyDisplaySlowNotes
        {
            get => earlyDisplaySlowNotes_bf;
            set {
                if (Utils.SetField(ref earlyDisplaySlowNotes_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsEarlyDisplaySlowNotes)));
                }
            }
        }


        private const float ZeroAvoidHighlightedSpeed = 0.1f;

        private float _highlightedNoteSpeed_bf;
        /// <summary>
        /// If <see cref="IsFilterNoteSpeed"/> is <see langword="true"/>,
        /// a downplayed note will not be selectable on stage.
        /// <br/>
        /// The value is also the default value when place note by editor
        /// </summary>
        public float HighlightedNoteSpeed
        {
            get => _highlightedNoteSpeed_bf;
            set {
                if (value <= 0f)
                    value = ZeroAvoidHighlightedSpeed;
                if (Utils.SetField(ref _highlightedNoteSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(HighlightedNoteSpeed)));
                }
            }
        }

        private bool _filterNoteSpeed;
        public bool IsFilterNoteSpeed
        {
            get => _filterNoteSpeed;
            set {
                if (Utils.SetField(ref _filterNoteSpeed, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsFilterNoteSpeed)));
                }
            }
        }

        private bool _applySpeedDiff_bf;
        public bool IsApplySpeedDifference
        {
            get => _applySpeedDiff_bf;
            set {
                if (Utils.SetField(ref _applySpeedDiff_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsApplySpeedDifference)));
                }
            }
        }

        #endregion

        #region Grid Properties

        private bool _timeGridsVisible_bf;
        public bool IsTimeGridsVisible
        {
            get => _timeGridsVisible_bf;
            set {
                if (Utils.SetField(ref _timeGridsVisible_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsTimeGridsVisible)));
                }
            }
        }

        private bool _positionGridsVisible_bf;
        public bool IsPositionGridsVisible
        {
            get => _positionGridsVisible_bf;
            set {
                if (Utils.SetField(ref _positionGridsVisible_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(IsPositionGridsVisible)));
                }
            }
        }

        #endregion

        #region Customization Properties

        private Color? _customSubBeatLineColor_bf;
        public Color? CustomSubdivisionLineColor
        {
            get => _customSubBeatLineColor_bf;
            set {
                if (Utils.SetField(ref _customSubBeatLineColor_bf, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(CustomSubdivisionLineColor)));
                }
            }
        }

        private Color? _customBeatLineColor;
        public Color? CustomBeatLineColor
        {
            get => _customBeatLineColor;
            set {
                if (Utils.SetField(ref _customBeatLineColor, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(CustomBeatLineColor)));
                }
            }
        }

        private Color? _customTempoLineColor;
        public Color? CustomTempoLineColor
        {
            get => _customTempoLineColor;
            set {
                if (Utils.SetField(ref _customTempoLineColor, value)) {
                    PropertyChanged?.Invoke(this, new(nameof(CustomTempoLineColor)));
                }
            }
        }

        #endregion

        public event Action<GameStageContext, PropertyEventArgs>? PropertyChanged;

        public GameStageContext(ProjectContext project, GamePlayContext gamePlay, IPerspectiveViewPanelInfoProvider perspectiveViewPanelInfo, SaveSystem storage)
        {
            ProjectContext = project;
            ThemeContext = new GameStageThemeContext();
            NotesContext = new GameStageNotesContext(this, project, gamePlay);
            PerspectiveViewPanelInfo = perspectiveViewPanelInfo;

            storage.SavingConfigurations += configs =>
            {
                configs.Set("stage/highlight_note_speed", HighlightedNoteSpeed);
                configs.Set("stage/filter_note_speed", IsFilterNoteSpeed);
                configs.Set("stage/apply_speed_diff", IsApplySpeedDifference);

                configs.Set("stage/note_speed", NoteFallSpeed);
                configs.Set("stage/show_link_lines", IsShowLinkLines);
                configs.Set("stage/piano_note_distinguish", IsPianoNotesDistinguished);
                configs.Set("stage/effect", IsStageEffectOn);
                configs.Set("stage/sudden_plus", SuddenPlus);
                configs.Set("stage/early_display_slow_notes", IsEarlyDisplaySlowNotes);
                //configs.Set("stage/ignore_note_speed_property", IgnoreNoteSpeed);

                configs.Set("stage/grids/pos_grid_visible", IsPositionGridsVisible);
                configs.Set("stage/grids/time_grid_visible", IsTimeGridsVisible);

                configs.Set("stage/line-color-subbeat", CustomSubdivisionLineColor?.ToRgbaString());
                configs.Set("stage/line-color-beat", CustomBeatLineColor?.ToRgbaString());
                configs.Set("stage/line-color-tempo", CustomTempoLineColor?.ToRgbaString());
            };

            storage.LoadedConfigurations += configs =>
            {
                HighlightedNoteSpeed = configs.GetSingle("stage/highlight_note_speed", 1f);
                IsFilterNoteSpeed = configs.GetBoolean("stage/filter_note_speed", false);
                IsApplySpeedDifference = configs.GetBoolean("stage/apply_speed_diff", true);

                NoteFallSpeed = configs.GetInt32("stage/note_speed", 40);
                IsShowLinkLines = configs.GetBoolean("stage/show_link_lines", true);
                IsPianoNotesDistinguished = configs.GetBoolean("stage/piano_note_distinguish", true);
                IsStageEffectOn = configs.GetBoolean("stage/effect", true);
                SuddenPlus = configs.GetSingle("stage/sudden_plus", 0f);
                IsEarlyDisplaySlowNotes = configs.GetBoolean("stage/early_display_slow_notes", false);
                //IgnoreNoteSpeed = configs.GetBoolean("stage/ignore_note_speed_property", false);

                IsPositionGridsVisible = configs.GetBoolean("stage/grids/pos_grid_visible", true);
                IsTimeGridsVisible = configs.GetBoolean("stage/grids/time_grid_visible", true);


                if (ColorUtils.TryParse(configs.GetString("stage/line-color-subbeat"), out var sbc)) {
                    CustomSubdivisionLineColor = sbc;
                }
                if (ColorUtils.TryParse(configs.GetString("stage/line-color-beat"), out var bc)) {
                    CustomBeatLineColor = bc;
                }
                if (ColorUtils.TryParse(configs.GetString("stage/line-color-tempo"), out var tc)) {
                    CustomTempoLineColor = tc;
                }
            };
        }

        internal float GetDisplaySpeed(float noteSpeed)
            => IsApplySpeedDifference ? noteSpeed : 1f;

        public bool IsNoteHighlighted(INoteSpeed note)
        {
            return !IsFilterNoteSpeed || Mathf.Approximately(note.Speed, HighlightedNoteSpeed);
        }

        public bool IsNoteDownplayed(INoteSpeed note)
        {
            return IsFilterNoteSpeed && !Mathf.Approximately(note.Speed, HighlightedNoteSpeed);
        }

        private static float ConvertToActualNoteSpeed(int noteSpeed) => noteSpeed / 10f;
    }
}
