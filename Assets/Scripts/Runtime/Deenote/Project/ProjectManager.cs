using CommunityToolkit.Diagnostics;
using Cysharp.Threading.Tasks;
using Deenote.Core;
using Deenote.CoreB.Helpers;
using Deenote.CoreB.Notification;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

        public ChartModel? CurrentChart
        {
            get => MainSystem.GamePlayManager.CurrentChart;
        }

        // TODO: R1.1 Audio或许不应该放这里，放GamePlay？
        private AudioClip? _audioClip;
        public AudioClip? CurrentAudioClip
        {
            get => _audioClip;
            [Obsolete("temp public")]
            internal set {
                if (_audioClip != value) {
                    _audioClip = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentAudioClip)));
                }
            }
        }

        public event Action<ProjectManager2, PropertyChangedEventArgs>? PropertyChanged;

        private CancellationTokenSource? _openLoadCts = new();
        private CancellationTokenSource _saveCts = new();

        public bool IsProjectSaving { get; private set; }
        public event Action<ProjectSaveEventArgs>? ProjectSaved;

        public void OpenProject(ProjectModel project, AudioClip clip)
        {
            if (_openLoadCts is not null) {
                _openLoadCts?.Cancel();
                _openLoadCts?.Dispose();
                _openLoadCts = null;
            }

            if (CurrentProject != project) {
                CurrentProject = project;
                if (project is not null)
                    project.AudioLength = clip.length;
                CurrentAudioClip = clip;
            }
        }

        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            _openLoadCts?.Cancel();
            _openLoadCts?.Dispose();
            _openLoadCts = new();

            using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _openLoadCts.Token);

            var proj = await ProjectIO.LoadAsync(filePath, lcts.Token);
            if (proj == null)
                return false;

            using var ms = new MemoryStream(proj.AudioFileData);
            var clip = await AudioUtils.TryLoadAsync(ms, Path.GetExtension(proj.AudioFileRelativePath));
            if (clip is null)
                return false;

            OpenProject(proj, clip);
            return true;
        }

        public void UnloadCurrentProject()
        {
            OpenProject(null!, null!);
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

        public UniTask SaveCurrentProjectToAsync(string targetFilePath, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(CurrentProject);

            _saveCts?.Cancel();
            _saveCts?.Dispose();
            _saveCts = new();

            using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _saveCts.Token);
            return SaveCurrentProjectAsyncInternal(targetFilePath, lcts.Token);
        }

        public UniTask SaveCurrentProjectAsync(CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(CurrentProject);

            _saveCts?.Cancel();
            _saveCts?.Dispose();
            _saveCts = new();

            using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _saveCts.Token);
            return SaveCurrentProjectAsyncInternal(CurrentProject.ProjectFilePath, lcts.Token);
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
