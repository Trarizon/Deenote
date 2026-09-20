using Deenote.CoreB.Notification;
using Deenote.Library;
using System;
using System.Drawing;

namespace Deenote.GameStage
{
    partial class GameStageManager : INotifyPropertyChanged<GameStageManager>
    {
        public event Action<GameStageManager, PropertyChangedEventArgs>? PropertyChanged;

        private const int MinNotFallSpeed = 5;
        private const int MaxNotFallSpeed = 95;

        private int _noteSpeed_bf;
        /// <summary>
        /// Range [5, 95], display [0.5, 9.5]
        /// </summary>
        public int NoteFallSpeed
        {
            get => _noteSpeed_bf;
            set {
                value = Math.Clamp(value, MinNotFallSpeed, MaxNotFallSpeed);
                if (Utils.SetField(ref _noteSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(NoteFallSpeed));
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
                    PropertyChanged?.Invoke(this, nameof(IsShowLinkLines));
                }
            }
        }

        private bool _distinguishPianoNotes_bf;
        public bool IsPianoNotesDistinguished
        {
            get => _distinguishPianoNotes_bf;
            set {
                if (Utils.SetField(ref _distinguishPianoNotes_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsPianoNotesDistinguished));
                }
            }
        }

        private bool _stageEffect_bf;
        public bool IsStageEffectOn
        {
            get => _stageEffect_bf;
            set {
                if (Utils.SetField(ref _stageEffect_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsStageEffectOn));
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
                value = Math.Clamp(value, 0, 1);
                if (Utils.SetField(ref _suddenPlus_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(SuddenPlus));
                }
            }
        }

        private bool _earlyDisplaySlowNotes_bf;
        /// <remarks>
        /// In DEEMO II, if a low-speed notes is following a high-speed note, 
        /// the slow note will appear only when the fast one appeared.
        /// That means, the slow note will appear from the center of note panel.
        /// </remarks>
        public bool IsEarlyDisplaySlowNotes
        {
            get => _earlyDisplaySlowNotes_bf;
            set {
                if (Utils.SetField(ref _earlyDisplaySlowNotes_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsEarlyDisplaySlowNotes));
                }
            }
        }

        // HighlightedNoteSpeed

        // ActualPlacementNoteSpeed;

        // TODO: arch: 似乎可以和HighlightedNoteSpeed合并起来
        private bool _filterNoteSpeed_bf;
        public bool IsFilterNoteSpeed
        {
            get => _filterNoteSpeed_bf;
            set {
                if (Utils.SetField(ref _filterNoteSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsFilterNoteSpeed));
                }
            }
        }

        private bool _applySpeedDiff_bf;
        public bool IsApplySpeedDifference
        {
            get => _applySpeedDiff_bf;
            set {
                if (Utils.SetField(ref _applySpeedDiff_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsApplySpeedDifference));
                }
            }
        }

        private static float ConvertToActualNoteSpeed(int noteSpeed) => noteSpeed / 10f;

        #region Indicator Properties

        private bool _indicatorsVisible_bf;
        public bool IsIndicatorsVisible
        {
            get => _indicatorsVisible_bf;
            set {
                if (Utils.SetField(ref _indicatorsVisible_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsIndicatorsVisible));
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
                    PropertyChanged?.Invoke(this, nameof(IsTimeGridsVisible));
                }
            }
        }

        private bool _positionGridsVisible_bf;
        public bool IsPositionGridsVisible
        {
            get => _positionGridsVisible_bf;
            set {
                if (Utils.SetField(ref _positionGridsVisible_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(IsPositionGridsVisible));
                }
            }
        }

        #endregion

        #region Customization Properties

        private Color? _customSubdivisionLineColor_bf;
        public Color? CustomSubdivisionLineColor
        {
            get => _customSubdivisionLineColor_bf;
            set {
                if (Utils.SetField(ref _customSubdivisionLineColor_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(CustomSubdivisionLineColor));
                }
            }
        }

        private Color? _customBeatLineColor_bf;
        public Color? CustomBeatLineColor
        {
            get => _customBeatLineColor_bf;
            set {
                if (Utils.SetField(ref _customBeatLineColor_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(CustomBeatLineColor));
                }
            }
        }

        private Color? _customTempoLineColor_bf;
        public Color? CustomTempoLineColor
        {
            get => _customTempoLineColor_bf;
            set {
                if (Utils.SetField(ref _customTempoLineColor_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(CustomTempoLineColor));
                }
            }
        }

        #endregion
    }
}