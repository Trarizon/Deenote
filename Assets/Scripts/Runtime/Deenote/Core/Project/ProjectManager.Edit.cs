#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Models.Charts;
using Deenote.Editing.EditorModels;
using System;
using System.Threading;

namespace Deenote.Core.Project
{
    partial class ProjectManager
    {
        [Obsolete]
        private UniTask<bool> TryEditProjectAudioAsync(string audioFilePath, CancellationToken cancellationToken = default)
        {
            ValidateProject();

            return CurrentProject.TrySetAudioByFilePathAsync(audioFilePath, cancellationToken);
        }

        [Obsolete]
        private void EditProjectMusicName(string name)
        {
            ValidateProject();

            CurrentProject.MusicName = name;
            NotifyFlag(NotificationFlag.ProjectMusicName);
        }

        [Obsolete]
        private void EditProjectComposer(string composer)
        {
            ValidateProject();

            CurrentProject.Composer = composer;
            NotifyFlag(NotificationFlag.ProjectComposer);
        }

        [Obsolete]
        private void EditProjectChartDesigner(string chartDesigner)
        {
            ValidateProject();

            CurrentProject.ChartDesigner = chartDesigner;
            NotifyFlag(NotificationFlag.ProjectChartDesigner);
        }

        [Obsolete]
        private ChartEditorModel AddProjectChart(ChartModel chart)
        {
            ValidateProject();

            var editorModel = new ChartEditorModel(chart);
            CurrentProject.Charts.Add(editorModel);
            NotifyFlag(NotificationFlag.ProjectCharts);
            return editorModel;
        }

        private void AddProjectChart(ChartEditorModel chart)
        {
            ValidateProject();

            CurrentProject.Charts.Add(chart);
            NotifyFlag(NotificationFlag.ProjectCharts);
        }

        [Obsolete]
        private void RemoveProjectChartAt(int chartIndex)
        {
            ValidateProject();

            CurrentProject.Charts.RemoveAt(chartIndex);
            if (CurrentProject.Charts.Count == 0)
                AddProjectChart(new ChartEditorModel());

            NotifyFlag(NotificationFlag.ProjectCharts);
        }
    }
}