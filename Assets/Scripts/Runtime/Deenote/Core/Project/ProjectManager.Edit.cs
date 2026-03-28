#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Models.Charts;
using Deenote.Editing.EditorModels;
using System.Threading;

namespace Deenote.Core.Project
{
    partial class ProjectManager
    {
        public async UniTask<bool> TryEditProjectAudioAsync(string audioFilePath, CancellationToken cancellationToken = default)
        {
            ValidateProject();

            var result = await CurrentProject.TrySetAndLoadAudioAsync(audioFilePath, cancellationToken);
            if (result) {
                NotifyFlag(NotificationFlag.ProjectAudio);
                return true;
            }
            else {
                return false;
            }

        }

        public void EditProjectMusicName(string name)
        {
            ValidateProject();

            CurrentProject.MusicName = name;
            NotifyFlag(NotificationFlag.ProjectMusicName);
        }

        public void EditProjectComposer(string composer)
        {
            ValidateProject();

            CurrentProject.Composer = composer;
            NotifyFlag(NotificationFlag.ProjectComposer);
        }

        public void EditProjectChartDesigner(string chartDesigner)
        {
            ValidateProject();

            CurrentProject.ChartDesigner = chartDesigner;
            NotifyFlag(NotificationFlag.ProjectChartDesigner);
        }

        public ChartEditorModel AddProjectChart(ChartModel chart)
        {
            ValidateProject();

            var editorModel= new ChartEditorModel(chart);
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

        public void RemoveProjectChartAt(int chartIndex)
        {
            ValidateProject();

            CurrentProject.Charts.RemoveAt(chartIndex);
            if (CurrentProject.Charts.Count == 0)
                AddProjectChart(new ChartEditorModel());

            NotifyFlag(NotificationFlag.ProjectCharts);
        }
    }
}