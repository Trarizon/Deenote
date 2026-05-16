#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Contexts;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views.Elements;
using Deenote.UIFramework;
using Deenote.UIFramework.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class ProjectInfoNavigationPageView : MonoBehaviour
    {
        private ProjectContext _projectContext;
        internal EnvironmentContext _environment;

        [Header("Project")]
        [SerializeField] RectTransform _projectInfoGroup = default!;
        [SerializeField] Button _audioButton = default!;
        [SerializeField] TextBox _musicNameInput = default!;
        [SerializeField] TextBox _composerInput = default!;
        [SerializeField] TextBox _chartDesignerInput = default!;
        [SerializeField] Collapsable _chartsCollapsable = default!;
        [SerializeField] Button _addChartButton = default!;
        [SerializeField] Button _loadChartButton = default!;
        [Header("Chart")]
        [SerializeField] RectTransform _chartInfoGroup = default!;
        [SerializeField] TextBox _chartNameInput = default!;
        [SerializeField] Dropdown _chartDifficultyDropdown = default!;
        [SerializeField] TextBox _chartLevelInput = default!;
        [SerializeField] TextBox _chartSpeedInput = default!;
        [SerializeField] TextBox _chartRemapMinVolumeInput = default!;
        [SerializeField] TextBox _chartRemapMaxVolumeInput = default!;
        [SerializeField] TextBox _chartConcatOffsetInput = default!;
        [SerializeField] TextBox _chartConcatMultiplierInput = default!;
        [SerializeField] Button _chartConcatLoadButton = default!;

        [Header("Prefabs")]
        [SerializeField] ProjectInfoChartListItem _chartListItemPrefab = default!;
        private PooledObjectListView<ProjectInfoChartListItem> _chartItems = default!;

        private ResettableCancellationTokenSource _rcts = default!;

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _loadAudioFailedMsgBoxArgs = new(
            LocalizableText.Localized("LoadAudio_MsgBox_Title"),
            LocalizableText.Localized("LoadAudioFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadAudioFailed_MsgBox_Y"));

        private static readonly MessageBoxArgs _loadChartFailedMsgBoxArgs = new(
            LocalizableText.Localized("LoadChart_MsgBox_Title"),
            LocalizableText.Localized("LoadChartFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadChartFailed_MsgBox_Y"));

        #endregion

        #region LocalizedTextKey

        private const string SelectAudioFileExplorerTitleKey = "SelectAudio_FileExplorer_Title";
        private const string SelectChartFileExplorerTitleKey = "SelectChart_FileExplorer_Title";
        private const string ChartLoadedStatusKey = "LoadChart_Status_Loaded";
        private const string LoadAudioLoadingStatusKey = "LoadAudio_Status_Loading";
        private const string LoadAudioLoadedStatusKey = "LoadAudio_Status_Loaded";

        #endregion

        #region Dropdown


        private string[] DifficultyDropdownOptions = new string[] {
            Difficulty.Easy.ToCapitalizedString(GameVersion.Deemo),
            Difficulty.Normal.ToCapitalizedString(GameVersion.Deemo),
            Difficulty.Hard.ToCapitalizedString(GameVersion.Deemo),
            Difficulty.Extra.ToCapitalizedString(GameVersion.Deemo),
        };

        private static Difficulty DropdownIndexToDifficulty(int index) => index switch {
            0 => Difficulty.Easy,
            1 => Difficulty.Normal,
            2 => Difficulty.Hard,
            3 => Difficulty.Extra,
            _ => throw new SwitchExpressionException(index)
        };

        private static int ToDropdownIndex(Difficulty difficulty) => difficulty switch {
            Difficulty.Easy => 0,
            Difficulty.Normal => 1,
            Difficulty.Hard => 2,
            Difficulty.Extra => 3,
            _ => throw new SwitchExpressionException(difficulty)
        };

        #endregion

        private float _chartConcatOffset = 0f;
        private float _chartConcatMultiplier = 1f;

        private void Awake()
        {
            _projectContext = MainSystem.Contexts.Project;
            _environment = MainSystem.Contexts.Environment;

            _chartItems = new(UnityUtils.CreateObjectPool(_chartListItemPrefab, _chartsCollapsable.Content,
                item => item.OnInstantiate(this), defaultCapacity: 0));
            _rcts = new();

            #region Project
            {
                _audioButton.Clicked += UniTask.Action(async () =>
                {
                    AssertProjectLoaded();
                    _rcts.CancelAndReset();
                    var cancellationToken = _rcts.Token;

                    using var ctr = cancellationToken.Register(() => MainWindow.StatusBar.SetReadyStatusMessage());

                    while (true) {
                        var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                            LocalizableText.Localized(SelectAudioFileExplorerTitleKey),
                            MainSystem.Args.SupportLoadAudioFileExtensions);
                        if (res.IsCancelled)
                            return;

                        var fileName = Path.GetFileName(res.Path);
                        MainWindow.StatusBar.SetLocalizedStatusMessage(LoadAudioLoadingStatusKey, fileName);

                        var loaded = await _projectContext.CurrentProject.TrySetAudioByFilePathAsync(res.Path, cancellationToken);
                        if (!loaded) {
                            MainWindow.StatusBar.SetReadyStatusMessage();

                            cancellationToken.ThrowIfCancellationRequested();
                            var btn = await MainWindow.DialogManager.OpenMessageBoxAsync(_loadAudioFailedMsgBoxArgs);
                            if (btn != 0)
                                return;
                            continue; // Re-select file
                        }

                        MainWindow.StatusBar.SetLocalizedStatusMessage(LoadAudioLoadedStatusKey, fileName, 3f);

                        break;
                    }
                });
                _musicNameInput.EditSubmitted += v => _projectContext.CurrentProject.MusicName = v;
                _composerInput.EditSubmitted += v => _projectContext.CurrentProject.Composer = v;
                _chartDesignerInput.EditSubmitted += v => _projectContext.CurrentProject.ChartDesigner = v;
                _addChartButton.Clicked += () =>
                {
                    var newChart = new ChartModel {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    };
                    var editorModel = _projectContext.CurrentProject.AddChart(newChart);
                    LoadChartModelToStage(editorModel);
                };
                _loadChartButton.Clicked += UniTask.Action(async () =>
                {
                    var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                        LocalizableText.Localized(SelectChartFileExplorerTitleKey),
                        MainSystem.Args.SupportLoadChartFileExtensions);
                    if (res.IsCancelled)
                        return;

                    if (!ChartData.TryParse(await File.ReadAllTextAsync(res.Path), out var chartData)) {
                        await MainWindow.DialogManager.OpenMessageBoxAsync(_loadChartFailedMsgBoxArgs);
                        return;
                    }

                    var chart = new ChartModel(chartData) {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    };

                    var editorChart = _projectContext.CurrentProject.AddChart(chart);
                    LoadChartModelToStage(editorChart);
                });

                _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.CurrentProject))) {
                        _projectInfoGroup.gameObject.SetActive(s.CurrentProject is not null);
                    }
                });
                _projectContext.RegisterNestedPropertyChangedAndInvoke(s => s.CurrentProject, nameof(ProjectContext.CurrentProject), (s, e) =>
                {
                    if (e.MatchProperty(nameof(s.AudioFileRelativePath))) {
                        _audioButton.Text.SetRawText(Path.GetFileName(s.AudioFileRelativePath));
                    }
                    if (e.MatchProperty(nameof(s.MusicName))) {
                        _musicNameInput.SetValueWithoutNotify(s.MusicName);
                    }
                    if (e.MatchProperty(nameof(s.Composer))) {
                        _composerInput.SetValueWithoutNotify(s.Composer);
                    }
                    if (e.MatchProperty(nameof(s.ChartDesigner))) {
                        _chartDesignerInput.SetValueWithoutNotify(s.ChartDesigner);
                    }
                    if (e.MatchProperty(nameof(s.Charts))) {
                        var charts = s.Charts;
                        using (var resetter = _chartItems.Resetting(charts.Count)) {
                            foreach (var chart in charts) {
                                resetter.Add(out var item);
                                item.Initialize(chart);
                            }
                        }
                        _chartItems.SetSiblingIndicesInOrder();
                    }
                });
            }
            #endregion

            #region Chart
            {
                _chartNameInput.EditSubmitted += (text) =>
                {
                    if (_projectContext.CurrentChart is { } chart) {
                        chart.Name = text;
                    }
                };
                _chartDifficultyDropdown.ResetOptions(DifficultyDropdownOptions);
                _chartDifficultyDropdown.SelectedIndexChanged += val =>
                {
                    var diff = DropdownIndexToDifficulty(val);
                    if (_projectContext.CurrentChart is { } chart) {
                        chart.Difficulty = diff;
                    }
                    _chartNameInput.SetPlaceHolderText(LocalizableText.Raw(diff.ToCapitalizedString(_environment.GameVersion)));
                };
                _chartLevelInput.EditSubmitted += text =>
                {
                    if (_projectContext.CurrentChart is { } chart) {
                        chart.Level = text;
                    }
                };
                _chartSpeedInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var speed)) {
                        if (_projectContext.CurrentChart is { } chart) {
                            chart.Speed = speed;
                        }
                    }
                    _chartSpeedInput.SetValueWithoutNotify(_projectContext.CurrentChart?.Speed.ToString("F3"));
                };
                _chartRemapMinVolumeInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var vol)) {
                        if (_projectContext.CurrentChart is { } chart) {
                            chart.RemapMinVolume = vol;
                        }
                    }
                    _chartRemapMinVolumeInput.SetValueWithoutNotify(_projectContext.CurrentChart?.RemapMinVolume.ToString());
                };
                _chartRemapMaxVolumeInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var vol)) {
                        if (_projectContext.CurrentChart is { } chart) {
                            chart.RemapMaxVolume = vol;
                        }
                    }
                    _chartRemapMaxVolumeInput.SetValueWithoutNotify(_projectContext.CurrentChart?.RemapMaxVolume.ToString());
                };

                _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
                {
                    if (e.MatchProperty(nameof(s.CurrentChart))) {
                        _chartInfoGroup.gameObject.SetActive(s.CurrentChart is not null);
                    }
                });

                _projectContext.RegisterNestedPropertyChangedAndInvoke(s => s.CurrentChart, nameof(ProjectContext.CurrentChart), (s, e) =>
                {
                    bool requireRefresh = false;
                    if (e.MatchProperty(nameof(s.Name))) {
                        SetName(s.Name);
                        requireRefresh = true;
                    }
                    if (e.MatchProperty(nameof(s.Difficulty))) {
                        SetDifficulty(s.Difficulty);
                        requireRefresh = true;
                    }
                    if (e.MatchProperty(nameof(s.Level))) {
                        SetLevel(s.Level);
                        requireRefresh = true;
                    }
                    if (e.MatchProperty(nameof(s.Speed))) {
                        SetSpeed(s.Speed);
                    }
                    if (e.MatchProperty(nameof(s.RemapMinVolume))) {
                        SetRemapMinVolume(s.RemapMinVolume);
                    }
                    if (e.MatchProperty(nameof(s.RemapMaxVolume))) {
                        SetRemapMaxVolume(s.RemapMaxVolume);
                    }

                    if (requireRefresh) {
                        RefreshChartListUI();
                    }
                });

                _chartConcatOffsetInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var offset))
                        _chartConcatOffset = offset;
                    _chartConcatOffsetInput.SetValueWithoutNotify(_chartConcatOffset.ToString("F3"));
                };
                _chartConcatMultiplierInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var multiplier))
                        _chartConcatMultiplier = multiplier;
                    if (_chartConcatMultiplier <= 0)
                        _chartConcatMultiplier = 0.1f;
                    _chartConcatMultiplierInput.SetValueWithoutNotify(_chartConcatMultiplier.ToString("F3"));
                };
                _chartConcatLoadButton.Clicked += [Obsolete] async () =>
                {
                    if (_projectContext.CurrentChart is null)
                        return;

                Reselect:
                    var fileRes = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                        LocalizableText.Raw("Select file to concatenate"),
                        MainSystem.Args.SupportLoadChartFileExtensions);
                    if (fileRes.IsCancelled)
                        return;
                    var file = fileRes.Path;

                    if (!ChartData.TryParse(File.ReadAllText(file), out var chartData)) {
                        var button = await MainWindow.DialogManager.OpenMessageBoxAsync(new MessageBoxArgs(
                            LocalizableText.Raw("Load chart failed."),
                            LocalizableText.Raw("Failed to parse chart file, please select another file."),
                            LocalizableText.Raw("Reselect"),
                            LocalizableText.Raw("Cancel")));
                        if (button != 0)
                            return;
                        goto Reselect;
                    }

                    MainSystem.StageChartEditor.ConcatNotes(chartData, _chartConcatOffset, _chartConcatMultiplier);
                };

                void SetName(string name) => _chartNameInput.SetValueWithoutNotify(name);
                void SetDifficulty(Difficulty difficulty)
                {
                    _chartNameInput.SetPlaceHolderText(LocalizableText.Raw(difficulty.ToCapitalizedString(_environment.GameVersion)));
                    _chartDifficultyDropdown.SetValueWithoutNotify(ToDropdownIndex(difficulty));
                }
                void SetLevel(string level) => _chartLevelInput.SetValueWithoutNotify(level);
                void SetSpeed(float speed) => _chartSpeedInput.SetValueWithoutNotify(speed.ToString("F3"));
                void SetRemapMinVolume(int vol) => _chartRemapMinVolumeInput.SetValueWithoutNotify(vol.ToString());
                void SetRemapMaxVolume(int vol) => _chartRemapMaxVolumeInput.SetValueWithoutNotify(vol.ToString());
            }
            #endregion
        }

        private void RefreshChartListUI()
        {
            foreach (var item in _chartItems) {
                item.RefreshUI();
            }
        }

        #region Chart List Item Callbacks

        internal void LoadChartToStage(ProjectInfoChartListItem item)
            => LoadChartModelToStage(item.ChartModel);

        internal void RemoveChart(ProjectInfoChartListItem item)
        {
            MainSystem.ProjectManager.AssertProjectLoaded();

            int findIndex = _chartItems.IndexOf(item);
            Debug.Assert(findIndex >= 0, $"Try to remove a {nameof(ProjectInfoChartListItem)} that is not in the list");
            _chartItems.RemoveAt(findIndex);
            Debug.Assert(ReferenceEquals(item.ChartModel, MainSystem.ProjectManager.CurrentProject.Charts[findIndex]),
                "Chart in ProjectInfo page and in current project not match");
            _projectContext.CurrentProject.RemoveChartAt(findIndex);
        }

        #endregion

        private void LoadChartModelToStage(ChartEditorModel chart)
        {
            _projectContext.CurrentChart = chart;
            MainWindow.StatusBar.SetLocalizedStatusMessage(ChartLoadedStatusKey);
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        private void AssertProjectLoaded() => Debug.Assert(MainSystem.ProjectManager.CurrentProject is not null);
    }
}