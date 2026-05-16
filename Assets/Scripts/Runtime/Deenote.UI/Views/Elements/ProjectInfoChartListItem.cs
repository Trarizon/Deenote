#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Models;
using Deenote.Editing.EditorModels;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Deenote.UIFramework.Controls;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Views.Elements
{
    public sealed class ProjectInfoChartListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Button _button = default!;
        [SerializeField] UnityEngine.UI.Image _difficutyIconImage = default!;
        [SerializeField] TextBlock _nameText = default!;
        [SerializeField] TextBlock _levelText = default!;
        [SerializeField] Button _exportButton = default!;
        [SerializeField] Button _removeButton = default!;

        private bool _isHovering;
        private ProjectInfoNavigationPageView _parent = default!;
        public ChartEditorModel ChartModel { get; private set; } = default!;

        #region LocalizedTextKeys

        private const string ExportFileExplorerTitleKey = "ExportChart_FileExplorer_Title";
        private const string ExportingStatusKey = "ExportChart_Status_Exporting";
        private const string ExportedStatusKey = "ExportChart_Status_Exported";

        #endregion

        private readonly static MessageBoxArgs _removeConfirmMsgBoxArgs = new(
            LocalizableText.Localized("RemoveChart_MsgBox_Title"),
            LocalizableText.Localized("RemoveChart_MsgBox_Content"),
            LocalizableText.Localized("RemoveChart_MsgBox_Y"),
            LocalizableText.Localized("RemoveChart_MsgBox_N")) {
            HighlightColorSet = Button.ButtonColorSet.Caution
        };

        private void Awake()
        {
            _button.Clicked += () => _parent.LoadChartToStage(this);
            _removeButton.Clicked += UniTask.Action(async () =>
            {
                var confirm = await MainWindow.DialogManager.OpenMessageBoxAsync(
                    _removeConfirmMsgBoxArgs);
                if (confirm != 0)
                    return;

                _parent.RemoveChart(this);
            });
            _exportButton.Clicked += UniTask.Action(async () =>
            {
                MainSystem.ProjectManager.AssertProjectLoaded();

                var projFilePath = MainSystem.ProjectManager.CurrentProject.ProjectFilePath;

                var suffix = string.IsNullOrEmpty(ChartModel.Name)
                    ? ChartModel.Difficulty.ToLowerCaseString(_parent._environment.GameVersion)
                    : ChartModel.Name;
                var res = await MainWindow.DialogManager.OpenFileExplorerInputFileAsync(
                    LocalizableText.Localized(ExportFileExplorerTitleKey),
                    defaultInput: $"{Path.GetFileNameWithoutExtension(projFilePath)}.{suffix}",
                    fileExtension: MainSystem.Args.DeenotePreferChartExtension,
                    initialDirectory: Path.GetDirectoryName(projFilePath));
                if (res.IsCancelled)
                    return;

                MainWindow.StatusBar.SetLocalizedStatusMessage(ExportingStatusKey);
                var chart = ChartModel.ToData();
                await File.WriteAllTextAsync(res.Path, chart.ToJsonString());
                MainWindow.StatusBar.SetLocalizedStatusMessage(ExportedStatusKey);
            });
        }

        internal void OnInstantiate(ProjectInfoNavigationPageView view)
        {
            _parent = view;
        }

        internal void Initialize(ChartEditorModel chart)
        {
            ChartModel = chart;
            RefreshUI();
            DoVisualTransition();
        }

        public void RefreshUI()
        {
            var data = MainWindow.Views.PerspectiveViewPanelView.StageForeground.Config.Get(ChartModel.Difficulty);
            _difficutyIconImage.sprite = data.IconSprite;
            _nameText.TmpText.color = data.TextColor;
            _levelText.TmpText.color = data.TextColor;
            if (string.IsNullOrEmpty(ChartModel.Name)) {
                _nameText.SetRawText(ChartModel.Difficulty.ToCapitalizedString(_parent._environment.GameVersion));
                _nameText.TmpText.fontStyle |= FontStyles.Italic;
            }
            else {
                _nameText.SetRawText(ChartModel.Name);
                _nameText.TmpText.fontStyle &= ~FontStyles.Italic;
            }
            _levelText.SetRawText($"Lv {ChartModel.Level}");
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            DoVisualTransition();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            DoVisualTransition();
        }

        private void DoVisualTransition()
        {
            _exportButton.gameObject.SetActive(_isHovering);
            _removeButton.gameObject.SetActive(_isHovering);
        }
    }
}