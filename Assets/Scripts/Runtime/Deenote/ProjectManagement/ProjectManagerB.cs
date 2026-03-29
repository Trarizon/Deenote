#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Contexts;
using Deenote.CoreB.IO;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Projects;
using Deenote.CoreB.Notification;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using Deenote.Library.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.ProjectManagement
{
    public enum ProjectLoadingStatus { None, Loading }
    public enum ProjectSavingStatus { None, Saving }

    public sealed class ProjectManagerB : INotifyPropertyChanged<ProjectManagerB>
    {
        private readonly ProjectContext _context;
        private readonly EnvironmentContext _environment;

        internal ProjectManagerB(ProjectContext context, EnvironmentContext environment)
        {
            _context = context;
            _environment = environment;
        }

        public ProjectLoadingStatus LoadingStatus { get; private set; }
        public ProjectSavingStatus SavingStatus { get; private set; }
        public ProjectSavingStatus ChartsSavingStatus { get; private set; }


        private ResettableCancellationTokenSource _saveCts = new();
        private ResettableCancellationTokenSource _saveChartsCts = new();

        public event Action? ProjectSaved;
        public event Action<ProjectManagerB, PropertyEventArgs>? PropertyChanged;

        public async UniTask<bool> TrySetCurrentProjectAndLoadAudioAsync(ProjectModel project, string filePath)
        {
            var proj = new ProjectEditorModel(project, filePath);
            var loaded = await proj.LoadAudioClipAsync();
            if (loaded) {
                _context.CurrentProject = proj;
                return true;
            }
            return false;
        }

        public async UniTask<bool> OpenLoadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            LoadingStatus = ProjectLoadingStatus.Loading;
            try {
                var model = await ProjectIO.LoadAsync(filePath, cancellationToken);
                if (model is null)
                    return false;

                var project = new ProjectEditorModel(model, filePath);
                var clipLoaded = await project.LoadAudioClipAsync();
                if (!clipLoaded)
                    return false;

                _context.CurrentProject = project;
                return true;
            } finally {
                LoadingStatus = ProjectLoadingStatus.None;
            }
        }

        public void UnloadCurrentProject()
        {
            _context.CurrentProject = null;
        }

        public async UniTask SaveCurrentProjectAsync(CancellationToken cancellationToken = default)
        {
            if (_context.CurrentProject is null)
                return;

            await SaveProjectToAsync(_context.CurrentProject, _context.CurrentProject.ProjectFilePath, cancellationToken);
            ProjectSaved?.Invoke();
        }

        public async UniTask SaveCurrentProjectToAsync(string targetFilePath, CancellationToken cancellationToken = default)
        {
            if (_context.CurrentProject is null)
                return;

            await SaveProjectToAsync(_context.CurrentProject, targetFilePath, cancellationToken);
            ProjectSaved?.Invoke();
        }

        public async UniTask SaveCurrentProjectChartJsonsAsync(CancellationToken cancellationToken = default)
        {
            if (_context.CurrentProject is null)
                return;

            await SaveProjectChartJsonsToAsync(_context.CurrentProject, Path.GetDirectoryName(_context.CurrentProject.ProjectFilePath), cancellationToken);
            ProjectSaved?.Invoke();
        }

        public async UniTask SaveCurrentProjectChartJsonsToAsync(string targetDirectory, CancellationToken cancellationToken = default)
        {
            if (_context.CurrentProject is null)
                return;

            await SaveProjectChartJsonsToAsync(_context.CurrentProject, targetDirectory, cancellationToken);
            ProjectSaved?.Invoke();
        }

        private async UniTask SaveProjectToAsync(ProjectEditorModel project, string path, CancellationToken cancellationToken = default)
        {
            SavingStatus = ProjectSavingStatus.Saving;
            IDisposable? cts = null;
            try {
                _saveCts.CancelAndReset();
                var ct = _saveCts.Token;
                if (cancellationToken.CanBeCanceled) {
                    var lcts = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationToken);
                    cts = lcts;
                    ct = lcts.Token;
                }

                await ProjectIO.SaveAsync(project.ToModel(), path, ct);
            } finally {
                cts?.Dispose();
                SavingStatus = ProjectSavingStatus.None;
            }
        }

        private async UniTask SaveProjectChartJsonsToAsync(ProjectEditorModel project, string dir, CancellationToken cancellationToken = default)
        {
            ChartsSavingStatus = ProjectSavingStatus.Saving;
            IDisposable? cts = null;
            try {
                _saveChartsCts.CancelAndReset();
                var ct = _saveChartsCts.Token;
                if (cancellationToken.CanBeCanceled) {
                    var lcts = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationToken);
                    cts = lcts;
                    ct = lcts.Token;
                }

                var time = DateTime.Now;
                var filename = Path.GetFileNameWithoutExtension(project.ProjectFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var tasks = new Task[project.Charts.Count];
                for (var i = 0; i < project.Charts.Count; i++) {
                    var chart = project.Charts[i];
                    var chartName = string.IsNullOrEmpty(chart.Name) ? chart.Difficulty.ToLowerCaseString(_environment.GameVersion) : chart.Name;

                    tasks[i] = File.WriteAllTextAsync(
                        Path.Combine(dir, PathUtils.ReplaceInvalidFileNameChars($"{filename}.{chartName}.{time:yyMMddHHmmss}.json")),
                        chart.ToData().ToJsonString(), ct);
                }

                await Task.WhenAll(tasks);
            } finally {
                cts?.Dispose();
                ChartsSavingStatus = ProjectSavingStatus.None;
            }
        }
    }
}
