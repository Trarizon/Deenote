using CommunityToolkit.Diagnostics;
using Cysharp.Threading.Tasks;
using Deenote.Core;
using Deenote.CoreB.Notification;
using Deenote.EditorModels;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using ObservableCollections;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.Project
{
    public sealed partial class ProjectManager2 : INotifyPropertyChanged<ProjectManager2>
    {
        private ProjectModel? _currentProject;
        public string? ProjectFilePath => CurrentProject?.ProjectFilePath;

        public ProjectModel? CurrentProject
        {
            get => _currentProject;
            private set {
                if (_currentProject != value) {
                    _currentProject = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProject)));
                }
            }
        }

        private ProjectEditorModel? _currentProject2;
        public ProjectEditorModel? CurrentProject2
        {
            get => _currentProject2;
            private set {
                if (_currentProject2 != value) {
                    if (_currentProject2 is not null)
                        _currentProject2.Charts.CollectionChanged -= _OnChartsChanged;
                    _currentProject2 = value;
                    if (_currentProject2 is not null)
                        _currentProject2.Charts.CollectionChanged += _OnChartsChanged;
                    else
                        CurrentChart = null;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProject2)));
                }
            }
        }

        private ChartModel? _currentChart;
        public ChartModel? CurrentChart
        {
            get => _currentChart;
            set {
                if (_currentChart != value) {
                    _currentChart = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentChart)));
                }
            }
        }

        public event Action<ProjectManager2, PropertyChangedEventArgs>? PropertyChanged;

        private void _OnChartsChanged(in NotifyCollectionChangedEventArgs<ChartModel> e)
        {
            foreach (var item in e.GetOldItems()) {
                if (item == CurrentChart) {
                    this.CurrentChart = null;
                    return;
                }
            }
        }

        private CancellationTokenSource? _openLoadCts = new();
        private CancellationTokenSource _saveCts = new();

        private bool _loading_bf = false;
        public bool IsProjectLoading
        {
            get => _loading_bf;
            private set {
                if (_loading_bf != value) {
                    _loading_bf = value;
                    PropertyChanged?.Invoke(this, nameof(IsProjectLoading));
                }
            }
        }
        public bool IsProjectSaving { get; private set; }
        public event Action<ProjectSaveEventArgs>? ProjectSaved;

        public void OpenProjectModel(ProjectEditorModel model)
        {
            if (_openLoadCts is not null) {
                _openLoadCts?.Cancel();
                _openLoadCts?.Dispose();
                _openLoadCts = null;
            }

            CurrentProject2 = model;
        }

        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            _openLoadCts?.Cancel();
            _openLoadCts?.Dispose();
            _openLoadCts = new();

            IsProjectLoading = true;
            try {

                using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _openLoadCts.Token);

                var proj = await ProjectIO.LoadAsync(filePath, lcts.Token);
                if (proj == null)
                    return false;

                OpenProjectModel(new ProjectEditorModel(proj));
            } finally {
                IsProjectLoading = false;
            }

            return true;
        }

        public void UnloadCurrentProject()
        {
            OpenProjectModel(null!);
        }

        private async UniTask SaveCurrentProjectAsyncInternal(string targetFilePath, CancellationToken cancellationToken)
        {
            IsProjectSaving = true;
            try {
                Asserts.NotNull(CurrentProject);
                await ProjectIO.SaveAsync(CurrentProject, targetFilePath, cancellationToken);
                ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project, targetFilePath));
            } finally {
                IsProjectSaving = false;
            }
        }

        public async UniTask SaveCurrentProjectToAsync(string targetFilePath, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(CurrentProject);

            _saveCts?.Cancel();
            _saveCts?.Dispose();
            _saveCts = new();

            using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _saveCts.Token);
            await SaveCurrentProjectAsyncInternal(targetFilePath, lcts.Token);
        }

        public async UniTask SaveCurrentProjectAsync(CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(CurrentProject);

            _saveCts?.Cancel();
            _saveCts?.Dispose();
            _saveCts = new();

            using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _saveCts.Token);
            await SaveCurrentProjectAsyncInternal(CurrentProject.ProjectFilePath, lcts.Token);
        }

        private async UniTask SaveCurrentProjectChartJsonsToAsyncInternal(string targetDirectory, IChartJsonSaveStrategy strategy, IProgress<bool>? progress, CancellationToken cancellationToken)
        {
            Asserts.NotNull(CurrentProject);
            try {
                var time = DateTime.Now;
                if (!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);

                progress?.Report(false);

                var tasks = new Task[CurrentProject.Charts.Count];
                for (int i = 0; i < CurrentProject.Charts.Count; i++) {
                    var chart = CurrentProject.Charts[i];
                    var chartName = strategy.GetFileName(CurrentProject, chart, time);

                    tasks[i] = File.WriteAllTextAsync(
                        Path.Combine(targetDirectory, chartName),
                        chart.ToJsonString(),
                        cancellationToken
                    );
                }
                await Task.WhenAll(tasks);
                progress?.Report(true);

                ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons, targetDirectory));
            } finally {
                IsProjectSaving = false;
            }
        }

        public UniTask SaveCurrentProjectChartJsonsToAsync(string targetDirectory, CancellationToken cancellationToken = default)
        {
            return SaveCurrentProjectChartJsonsToAsync(targetDirectory, null, null, cancellationToken);
        }

        public UniTask SaveCurrentProjectChartJsonsToAsync(string targetDirectory, IChartJsonSaveStrategy? strategy, IProgress<bool>? progress = null, CancellationToken cancellationToken = default)
        {
            return SaveCurrentProjectChartJsonsToAsyncInternal(targetDirectory, strategy ?? ChartJsonSaveStrategy.Default, progress, cancellationToken);
        }
    }
}
