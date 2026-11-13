#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.CoreB.Notification;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class PlayerNavigationPageView : MonoBehaviour
    {
        [SerializeField] Dropdown _aspectRatioDropdown = default!;
        [SerializeField] Dropdown _renderScaleDropdown = default!;
        [SerializeField] Button _fullScreenButton = default!;
        [SerializeField] NumericStepper _noteSpeedNumericStepper = default!;
        [SerializeField] Slider _musicVolumeSlider = default!;
        [SerializeField] TextBox _musicVolumeInput = default!;
        [SerializeField] Slider _effectVolumeSlider = default!;
        [SerializeField] TextBox _effectVolumeInput = default!;
        [SerializeField] Slider _pianoVolumeSlider = default!;
        [SerializeField] TextBox _pianoVolumeInput = default!;
        [SerializeField] Slider _suddenPlusSlider = default!;
        [SerializeField] TextBox _suddenPlusInput = default!;
        [SerializeField] ToggleSwitch _linksIndicatorToggle = default!;
        [SerializeField] ToggleSwitch _warningNoteToggle = default!;
        [SerializeField] ToggleSwitch _warningLineToggle = default!;
        [SerializeField] ToggleSwitch _placementIndicatorToggle = default!;
        [SerializeField] ToggleSwitch _earlyDisplaySlowNotesToggle = default!;

        private GamePlayManager2 _gamePlayManager;
        private GameStageManager _stageManager;

        private void Awake()
        {
            _gamePlayManager = App.GamePlayManager;
            _stageManager = App.GameStageManager;
        }

        private void Start()
        {
            #region View Screen

            _aspectRatioDropdown.ResetOptions(_predefinedAspectTexts);
            _aspectRatioDropdown.SelectedIndexChanged += val =>
            {
                if (val >= 0)
                    MainWindow.Views.PerspectiveViewPanelView.AspectRatio = GetAspectRatioDropdownOption(val);
            };
            MainWindow.Views.PerspectiveViewPanelView.AspectRatioChanged += val => _aspectRatioDropdown.SetValueWithoutNotify(GetAspectRatioDropdownIndex(val));
            _aspectRatioDropdown.SetValueWithoutNotify(GetAspectRatioDropdownIndex(MainWindow.Views.PerspectiveViewPanelView.AspectRatio));

            _fullScreenButton.Clicked += () => MainWindow.Views.PerspectiveViewPanelView.SetIsFullScreen(true);

            _renderScaleDropdown.ResetOptions(_predefinedRenderScaleTexts);
            _renderScaleDropdown.SelectedIndexChanged += val =>
            {
                if (val >= 0)
                    MainWindow.Views.PerspectiveViewPanelView.RenderScale =
                        GetRenderScaleDropdownOption(val);
            };

            MainWindow.Views.PerspectiveViewPanelView.RenderScaleChanged += val =>
                _renderScaleDropdown.SetValueWithoutNotify(GetRenderScaleDropdownIndex(val));

            _renderScaleDropdown.SetValueWithoutNotify(
                GetRenderScaleDropdownIndex(MainWindow.Views.PerspectiveViewPanelView.RenderScale));
            #endregion

            #region NoteSpeed Volumes Sudden+

            static void Sync01Range(TextBox input, Slider slider, float value)
            {
                input.SetValueWithoutNotify((value * 100f).ToString("F0"));
                slider.SetValueWithoutNotify(value);
            }

            // Stage

            _noteSpeedNumericStepper.SetInputParser(static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
            _noteSpeedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
            _noteSpeedNumericStepper.ValueChanged += val => _stageManager.NoteFallSpeed = val;

            _suddenPlusSlider.ValueChanged += val => _stageManager.SuddenPlus = val;
            _suddenPlusInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _stageManager.SuddenPlus = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_suddenPlusInput, _suddenPlusSlider, _stageManager.SuddenPlus);
            };

            _stageManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.NoteFallSpeed))) {
                    _noteSpeedNumericStepper.SetValueWithoutNotify(_stageManager.NoteFallSpeed);
                }
                if (e.Match(nameof(s.SuddenPlus))) {
                    Sync01Range(_suddenPlusInput, _suddenPlusSlider, _stageManager.SuddenPlus);
                }
            }).UnregisterWhenGameObjectDestroyed(this);

            // GamePlay

            _musicVolumeSlider.ValueChanged += val => _gamePlayManager.MusicVolume = val;
            _musicVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _gamePlayManager.MusicVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_musicVolumeInput, _musicVolumeSlider, _gamePlayManager.MusicVolume);
            };
            _effectVolumeSlider.ValueChanged += val => _gamePlayManager.HitSoundVolume = val;
            _effectVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _gamePlayManager.HitSoundVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_effectVolumeInput, _effectVolumeSlider, _gamePlayManager.HitSoundVolume);
            };
            _pianoVolumeSlider.ValueChanged += val => _gamePlayManager.PianoVolume = val;
            _pianoVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _gamePlayManager.PianoVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_pianoVolumeInput, _pianoVolumeSlider, _gamePlayManager.PianoVolume);
            };

            _gamePlayManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.MusicVolume))) {
                    Sync01Range(_musicVolumeInput, _musicVolumeSlider, _gamePlayManager.MusicVolume);
                }
                if (e.Match(nameof(s.HitSoundVolume))) {
                    Sync01Range(_effectVolumeInput, _effectVolumeSlider, _gamePlayManager.HitSoundVolume);
                }
                if (e.Match(nameof(s.PianoVolume))) {
                    Sync01Range(_pianoVolumeInput, _pianoVolumeSlider, _gamePlayManager.PianoVolume);
                }
            }).UnregisterWhenGameObjectDestroyed(this);

            #endregion

            // Stage

            _linksIndicatorToggle.IsCheckedChanged += val => _stageManager.IsShowLinkLines = val;
            _warningNoteToggle.IsCheckedChanged += val =>
            {
                if (val)
                    _stageManager.WarningNoteVisibility |= GameStageWarningNoteVisibility.ShowNote;
                else
                    _stageManager.WarningNoteVisibility &= ~GameStageWarningNoteVisibility.ShowNote;

            };
            _warningLineToggle.IsCheckedChanged += val =>
            {
                if (val)
                    _stageManager.WarningNoteVisibility |= GameStageWarningNoteVisibility.ShowLine;
                else
                    _stageManager.WarningNoteVisibility &= ~GameStageWarningNoteVisibility.ShowLine;
            };
            _earlyDisplaySlowNotesToggle.IsCheckedChanged += val => _stageManager.IsEarlyDisplaySlowNotes = val;

            _stageManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.IsShowLinkLines))) {
                    _linksIndicatorToggle.SetIsCheckedWithoutNotify(_stageManager.IsShowLinkLines);
                }
                if (e.Match(nameof(s.WarningNoteVisibility))) {
                    _warningNoteToggle.SetIsCheckedWithoutNotify(_stageManager.WarningNoteVisibility.HasFlag(GameStageWarningNoteVisibility.ShowNote));
                    _warningLineToggle.SetIsCheckedWithoutNotify(_stageManager.WarningNoteVisibility.HasFlag(GameStageWarningNoteVisibility.ShowLine));
                }
                if (e.Match(nameof(s.IsEarlyDisplaySlowNotes))) {
                    _earlyDisplaySlowNotesToggle.SetIsCheckedWithoutNotify(_stageManager.IsEarlyDisplaySlowNotes);
                }
            }).UnregisterWhenGameObjectDestroyed(this);

            // Placer

            _placementIndicatorToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.IsIndicatorOn = val;

            MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                StageNotePlacer.NotificationFlag.IsIndicatorOn,
                placer => _placementIndicatorToggle.SetIsCheckedWithoutNotify(placer.IsIndicatorOn));
        }

        private static readonly string[] _predefinedAspectTexts = { "16:9", "16:10", "4:3" };

        private static float GetAspectRatioDropdownOption(int optionIndex)
            => optionIndex switch {
                0 => 16f / 9f,
                1 => 16f / 10f,
                2 => 4f / 3f,
                _ => ThrowHelper.ThrowInvalidOperationException<float>(),
            };

        private static int GetAspectRatioDropdownIndex(float option)
            => option switch {
                16f / 9f => 0,
                16f / 10f => 1,
                4f / 3f => 2,
                _ => -1,
            };

        private static readonly string[] _predefinedRenderScaleTexts =
        {
            "0.5x",
            "0.75x",
            "1.0x",
            "1.25x",
            "1.5x",
            "2.0x",
        };

        private static readonly float[] _predefinedRenderScales =
        {
            0.5f,
            0.75f,
            1.0f,
            1.25f,
            1.5f,
            2.0f,
        };

        private static float GetRenderScaleDropdownOption(int optionIndex)
        {
            if ((uint)optionIndex >= (uint)_predefinedRenderScales.Length)
                return ThrowHelper.ThrowInvalidOperationException<float>();

            return _predefinedRenderScales[optionIndex];
        }

        private static int GetRenderScaleDropdownIndex(float option)
        {
            for (int i = 0; i < _predefinedRenderScales.Length; i++) {
                if (Mathf.Approximately(option, _predefinedRenderScales[i]))
                    return i;
            }

            return -1;
        }
    }
}