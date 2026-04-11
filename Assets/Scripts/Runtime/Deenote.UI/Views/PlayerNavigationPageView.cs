#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.CoreB.Notification;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class PlayerNavigationPageView : MonoBehaviour
    {
        private GamePlayContext _gamePlayContext;
        private GameStageContext _stageContext;

        [SerializeField] Dropdown _aspectRatioDropdown = default!;
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
        [SerializeField] ToggleSwitch _placementIndicatorToggle = default!;
        [SerializeField] ToggleSwitch _earlyDisplaySlowNotesToggle = default!;

        private void Awake()
        {
            _gamePlayContext = MainSystem.Contexts.GamePlay;
            _stageContext = MainSystem.Contexts.GameStage;
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

            #endregion

            #region NoteSpeed Volumes Sudden+

            static void Sync01Range(TextBox input, Slider slider, float value)
            {
                input.SetValueWithoutNotify((value * 100f).ToString("F0"));
                slider.SetValueWithoutNotify(value);
            }

            _noteSpeedNumericStepper.SetInputParser(static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
            _noteSpeedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
            _noteSpeedNumericStepper.ValueChanged += val => _stageContext.NoteFallSpeed = val;
            _musicVolumeSlider.ValueChanged += val => _gamePlayContext.MusicVolume = val;
            _musicVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _gamePlayContext.MusicVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_musicVolumeInput, _musicVolumeSlider, _gamePlayContext.MusicVolume);
            };
            _effectVolumeSlider.ValueChanged += val => _gamePlayContext.HitSoundVolume = val;
            _effectVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _gamePlayContext.HitSoundVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_effectVolumeInput, _effectVolumeSlider, _gamePlayContext.HitSoundVolume);
            };
            _pianoVolumeSlider.ValueChanged += val => _gamePlayContext.PianoVolume = val;
            _pianoVolumeInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _gamePlayContext.PianoVolume = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_pianoVolumeInput, _pianoVolumeSlider, _gamePlayContext.PianoVolume);
            };
            _suddenPlusSlider.ValueChanged += val => _stageContext.SuddenPlus = val;
            _suddenPlusInput.EditSubmitted += input =>
            {
                if (int.TryParse(input, out var ival))
                    _stageContext.SuddenPlus = Mathf.Clamp01(ival / 100f);
                else
                    Sync01Range(_suddenPlusInput, _suddenPlusSlider, _stageContext.SuddenPlus);
            };

            _stageContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.NoteFallSpeed))) {
                    _noteSpeedNumericStepper.Value = s.NoteFallSpeed;
                }
                if (e.MatchProperty(nameof(s.SuddenPlus))) {
                    Sync01Range(_suddenPlusInput, _suddenPlusSlider, s.SuddenPlus);
                }
            });
            _gamePlayContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.MusicVolume))) {
                    Sync01Range(_musicVolumeInput, _musicVolumeSlider, s.MusicVolume);
                }
                if (e.MatchProperty(nameof(s.HitSoundVolume))) {
                    Sync01Range(_effectVolumeInput, _effectVolumeSlider, s.HitSoundVolume);
                }
                if (e.MatchProperty(nameof(s.PianoVolume))) {
                    Sync01Range(_pianoVolumeInput, _pianoVolumeSlider, s.PianoVolume);
                }
            });

            #endregion

            _linksIndicatorToggle.IsCheckedChanged += val => _stageContext.IsShowLinkLines = val;
            _placementIndicatorToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.IsIndicatorOn = val;
            _earlyDisplaySlowNotesToggle.IsCheckedChanged += val => _stageContext.IsEarlyDisplaySlowNotes = val;

            _stageContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.IsShowLinkLines))) {
                    _linksIndicatorToggle.SetIsCheckedWithoutNotify(s.IsShowLinkLines);
                }
                if (e.MatchProperty(nameof(s.IsEarlyDisplaySlowNotes))) {
                    _earlyDisplaySlowNotesToggle.SetIsCheckedWithoutNotify(s.IsEarlyDisplaySlowNotes);
                }
            });
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
    }
}