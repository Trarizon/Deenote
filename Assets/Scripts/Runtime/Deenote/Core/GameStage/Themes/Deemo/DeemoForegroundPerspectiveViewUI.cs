#nullable enable

using Deenote.Contexts;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage.Foreground;
using Deenote.Core.Project;
using Deenote.CoreB.Models;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.GameStage;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    public sealed class DeemoForegroundPerspectiveViewUI : ForegroundPerspectiveViewUI
    {
        private ProjectContext _project;
        private EnvironmentContext _environment;
        private GameStageContext _gameStage;

        [Header("Info Bar")]
        [SerializeField] TMP_Text _musicNameText = default!;
        [SerializeField] TMP_Text _scoreText = default!;
        [SerializeField] Slider _timeSlider = default!;
        [SerializeField] Image _difficultyImage = default!;
        [SerializeField] TMP_Text _levelText = default!;
        [SerializeField] Button _pauseButton = default!;
        [Header("Combo UI")]
        [SerializeField] GameObject _comboGameObject = default!;
        [SerializeField] TextMeshProUGUI _numberText = default!;
        [SerializeField] TextMeshProUGUI _shadowText = default!;
        [SerializeField] Image _shockWaveCircleImage = default!;
        [SerializeField] Image _shockWaveImage = default!;
        [SerializeField] RectTransform _shockWaveEnterPosTransform = default!;
        [SerializeField] RectTransform _shockWaveExitPosTransform = default!;
        [SerializeField] Image _charmingImage = default!;

        [Header("Resources")]
        [SerializeField] DeemoForegroundUIConfig _args = default!;

        private float _shockWaveEnterPosX;
        private float _shockWaveExitPosX;

        private Difficulty _difficulty_bf;
        private Difficulty Difficulty
        {
            get => _difficulty_bf;
            set {
                if (Utils.SetField(ref _difficulty_bf, value)) {
                    (_difficultyImage.sprite, _levelText.color) = base.Args.GetDifficultyArgs(value);
                    UpdateLevelText();
                }
            }
        }

        private string? _level_bf;
        private string Level
        {
            get => _level_bf!;
            set {
                if (Utils.SetField(ref _level_bf, value)) {
                    UpdateLevelText();
                }
            }
        }


        protected override void Awake()
        {
            _project = MainSystem.Contexts.Project;
            _environment = MainSystem.Contexts.Environment;
            _gameStage = MainSystem.Contexts.GameStage;

            _shockWaveEnterPosX = _shockWaveEnterPosTransform.anchorMin.x;
            _shockWaveExitPosX = _shockWaveExitPosTransform.anchorMin.x;
        }

        private void Start()
        {
            _timeSlider.onValueChanged.AddListener(val => MainSystem.GamePlayManager.MusicPlayer.Time = val);
            _pauseButton.onClick.AddListener(() => MainSystem.GamePlayManager.MusicPlayer.TogglePlayingState());

            MainSystem.GamePlayManager.MusicPlayer.ClipChanged += clip =>
            {
                if (clip is not null)
                    _timeSlider.maxValue = clip.length;
            };
            MainSystem.GamePlayManager.MusicPlayer.TimeChanged += args => _timeSlider.SetValueWithoutNotify(args.NewTime);
            _timeSlider.maxValue = MainSystem.GamePlayManager.MusicPlayer.ClipLength;
            _timeSlider.SetValueWithoutNotify(MainSystem.GamePlayManager.MusicPlayer.Time);

            _project.RegisterNestedPropertyChangedAndInvokeNullable(s => s.CurrentProject, nameof(_project.CurrentProject), (s, e) =>
            {
                if (e.MatchProperty(nameof(s.MusicName))) {
                    _musicNameText.text = s?.MusicName ?? "";
                }
            });

            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.ChartLevel,
                manager =>
                {
                    manager.AssertChartLoaded();
                    Level = manager.CurrentChart.Level;
                });

            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.ChartDifficulty,
                manager =>
                {
                    manager.AssertChartLoaded();
                    Difficulty = manager.CurrentChart.Difficulty;
                });

            _project.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    var chart = s.CurrentChart;
                    if (chart is null) {
                        gameObject.SetActive(false);
                    }
                    else {
                        Level = chart.Level;
                        Difficulty = chart.Difficulty;
                        gameObject.SetActive(true);
                    }
                }
            });

            _gameStage.NotesContext.Updated += (s) =>
            {
                if (_project.CurrentChart is null)
                    return;
                UpdateComboRegistrant(s);
            };
            _gameStage.NotesContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentCombo))) {
                    if (_project.CurrentChart is null)
                        return;
                    var currentCombo = s.CurrentCombo;
                    if (currentCombo <= 0) {
                        _scoreText.text = "0.00 %";
                        return;
                    }

                    var noteCount = _project.CurrentChart.Notes.Count;
                    float accScore = (float)currentCombo / noteCount;
                    // comboActual = Sum(1..judgeNoteCount);
                    // comboTotal = Sum(1..noteCount)
                    // comboScore = comboActual / comboTotal
                    //            = ((1 + judged) * judged) / ((1 + count) * count)
                    float comboScore = (float)((1 + currentCombo) * currentCombo) / ((1 + noteCount) * noteCount);

                    float score = accScore * 80_00f + comboScore * 20_00f;
                    _scoreText.text = $"{Mathf.Floor(score) / 100f:F2} %";
                }
            });

            //MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
            //    GamePlayManager.NotificationFlag.ActiveNoteUpdated,
            //    manager =>
            //    {
            //        if (manager.CurrentChart is not { } chart)
            //            return;

            //        UpdateComboRegistrant(manager);

            //        var currentCombo = manager.NotesManager.CurrentCombo;
            //        if (currentCombo <= 0) {
            //            _scoreText.text = "0.00 %";
            //            return;
            //        }

            //        int noteCount = chart.Notes.Count;
            //        float accScore = (float)currentCombo / noteCount;
            //        // comboActual = Sum(1..judgeNoteCount);
            //        // comboTotal = Sum(1..noteCount)
            //        // comboScore = comboActual / comboTotal
            //        //            = ((1 + judged) * judged) / ((1 + count) * count)
            //        float comboScore = (float)((1 + currentCombo) * currentCombo) / ((1 + noteCount) * noteCount);

            //        float score = accScore * 80_00f + comboScore * 20_00f;
            //        _scoreText.text = $"{Mathf.Floor(score) / 100f:F2} %";
            //    });
        }

        private void UpdateComboRegistrant(GameStageNotesContext context)
        {
            int combo = context.CurrentCombo;
            if (combo < _args.MinDisplayCombo) {
                _comboGameObject.SetActive(false);
                return;
            }

            // prevHitNoteIndex wont smaller than combo, so here
            // it is asserted a valid index
            var prevHitNote = context.GetPreviousHitComboNode();
            Debug.Assert(prevHitNote?.IsComboNode ?? false);

            _comboGameObject.SetActive(true);
            var deltaTime = context.CurrentTime - prevHitNote!.Time;
            Debug.Assert(deltaTime >= 0, $"actual delta time:{deltaTime}");
            _numberText.text = _shadowText.text = combo.ToString();

            // Number
            {
                float grey;
                if (deltaTime <= _args.ComboNumberGreyIncTime) {
                    float ratio = deltaTime / _args.ComboNumberGreyIncTime;
                    grey = Mathf.Lerp(0f, 1f, ratio);
                }
                else if (deltaTime <= _args.ComboNumberGreyIncTime + _args.ComboNumberGreyDecTime) {
                    float ratio = (deltaTime - _args.ComboNumberGreyIncTime) / _args.ComboNumberGreyDecTime;
                    grey = Mathf.Pow(1f - ratio, 0.67f);
                }
                else {
                    grey = 0f;
                }
                _numberText.color = new Color(grey, grey, grey);
            }
            // Shadow Number
            {
                float alpha;
                float scale;
                if (deltaTime <= _args.ComboShadowDuration) {
                    float ratio = deltaTime / _args.ComboShadowDuration;
                    alpha = Mathf.Lerp(1f, _args.ComboShadowMinAlpha, ratio);
                    scale = Mathf.Lerp(1f, _args.ComboShadowMaxScale, ratio);
                }
                else {
                    alpha = _args.ComboShadowMinAlpha;
                    scale = _args.ComboShadowMaxScale;
                }
                _shadowText.transform.localScale = new Vector3(scale, scale, scale);
                _shadowText.color = Color.black with { a = alpha };
            }
            // Shock Wave Circle
            {
                float alpha;
                float scale;
                if (deltaTime <= _args.ComboCircleScaleStartTime)
                    scale = 0f;
                else if (deltaTime <= _args.ComboCircleScaleEndTime)
                    scale = MathUtils.MapTo(deltaTime, _args.ComboCircleScaleStartTime, _args.ComboCircleScaleEndTime, 0f, _args.ComboCircleMaxScale);
                else
                    scale = 0f;

                if (deltaTime <= _args.ComboCircleFadeInStartTime)
                    alpha = 0f;
                else if (deltaTime <= _args.ComboCircleFadeInEndTime)
                    alpha = Mathf.InverseLerp(_args.ComboCircleScaleStartTime, _args.ComboCircleScaleEndTime, deltaTime);
                else if (deltaTime <= _args.ComboCircleFadeOutStartTime)
                    alpha = 1f;
                else
                    alpha = Mathf.InverseLerp(_args.ComboCircleScaleEndTime, _args.ComboCircleFadeOutStartTime, deltaTime);

                _shockWaveCircleImage.transform.localScale = new Vector3(scale, scale, scale);
                _shockWaveCircleImage.color = Color.white with { a = alpha };
            }
            // Shock Wave Strike
            {
                _shockWaveImage.rectTransform.WithAnchoredMinMaxX(GetStrikePosition(deltaTime));
                _shockWaveImage.WithColorAlpha(GetStrikeAlpha(deltaTime));

                float GetStrikePosition(float deltaTime)
                {
                    if (deltaTime <= _args.ComboShockWaveAlphaIncTime) {
                        return _shockWaveEnterPosX;
                    }
                    else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime) {
                        float ratio = (deltaTime - _args.ComboShockWaveAlphaIncTime) / _args.ComboShockWaveMoveTime;
                        return Mathf.Lerp(_shockWaveEnterPosX, _shockWaveExitPosX, ratio);
                    }
                    else {
                        return _shockWaveExitPosX;
                    }
                }

                float GetStrikeAlpha(float deltaTime)
                {
                    if (deltaTime <= _args.ComboShockWaveAlphaIncTime) {
                        float ratio = deltaTime / _args.ComboShockWaveAlphaIncTime;
                        return ratio;
                    }
                    else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime) {
                        return 1f;
                    }
                    else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime +
                    _args.ComboShockWaveAlphaDecTime) {
                        float ratio = (deltaTime - _args.ComboShockWaveAlphaIncTime - _args.ComboShockWaveMoveTime) /
                                      _args.ComboShockWaveAlphaDecTime;
                        return 1 - ratio;
                    }
                    else {
                        return 0;
                    }
                }
            }
            // Charming
            {
                float scale;
                if (deltaTime <= _args.ComboCharmingGrowTime) {
                    float ratio = deltaTime / _args.ComboCharmingGrowTime;
                    scale = Mathf.Lerp(1f, _args.ComboCharmingMaxScaleY, ratio);
                }
                else if (deltaTime <= _args.ComboCharmingGrowTime + _args.ComboCharmingFadeTime) {
                    float ratio = (deltaTime - _args.ComboCharmingGrowTime) / _args.ComboCharmingFadeTime;
                    scale = Mathf.Lerp(_args.ComboCharmingMaxScaleY, 1f, ratio);
                }
                else {
                    scale = 0f;
                }
                _charmingImage.transform.localScale = new Vector3(1f, scale, 1f);

                float alpha;
                if (deltaTime <= _args.ComboCharmingAlphaIncTime) {
                    float ratio = deltaTime / _args.ComboCharmingAlphaIncTime;
                    alpha = Mathf.Lerp(0f, 1f, ratio);
                }
                else if (deltaTime <= _args.ComboCharmingAlphaDecStartTime) {
                    alpha = 1f;
                }
                else if (deltaTime <= _args.ComboCharmingAlphaDecStartTime + _args.ComboCharmingAlphaDecTime) {
                    float ratio = (deltaTime - _args.ComboCharmingAlphaDecStartTime) / _args.ComboCharmingAlphaDecTime;
                    alpha = Mathf.Lerp(1f, 0f, ratio);
                }
                else {
                    alpha = 0f;
                }
                _charmingImage.color = Color.white with { a = alpha };
            }
        }

        private void UpdateLevelText()
        {
            _levelText.text = $"{Difficulty.ToCapitalizedString(_environment.GameVersion)} Lv {Level}";
        }
    }
}