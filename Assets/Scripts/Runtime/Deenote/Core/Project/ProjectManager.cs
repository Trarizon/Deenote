#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.IO;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Projects;
using Deenote.Editing.Contexts;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote.Core.Project
{
    public sealed partial class ProjectManager : FlagNotifiableMonoBehaviour<ProjectManager, ProjectManager.NotificationFlag>
    {
        private readonly EnvironmentContext _environment = new();

        private ProjectEditorModel? _currentProject_bf;
        public ProjectEditorModel? CurrentProject
        {
            get => _currentProject_bf;
        }

        //private AudioClip? _audioClip;
        public AudioClip? AudioClip => CurrentProject?.AudioClip;

        private bool _isLoading_bf;
        private bool _isSaving_bf;
        private ResettableCancellationTokenSource _saveCts = new();
        private ResettableCancellationTokenSource _saveChartsCts = new();

        public bool IsLoading
        {
            get => _isLoading_bf;
            private set {
                if (Utils.SetField(ref _isLoading_bf, value)) {
                    NotifyFlag(NotificationFlag.IsLoading);
                }
            }
        }

        public bool IsSaving
        {
            get => _isSaving_bf;
            private set {
                if (Utils.SetField(ref _isSaving_bf, value)) {
                    NotifyFlag(NotificationFlag.IsSaving);
                }
            }
        }

        private void Awake()
        {
            RegisterAutoSaveConfigurations();
        }

#if  UNITY_EDITOR
        private async void Start()
        {
            var proj = await Fake.GetProject();
            Debug.Log($"Loaded project: fake");
            SetCurrentProject(proj);
        }
#endif

        public async UniTask<bool> TrySetCurrentProjectAndLoadAudioAsync(ProjectModel project, string filePath)
        {
            var proj = new ProjectEditorModel(project, filePath);
            var loaded = await proj.LoadAudioClipAsync();
            if (loaded) {
                SetCurrentProject(proj);
                return true;
            }
            return false;
        }

        private void SetCurrentProject(ProjectEditorModel project)
        {
            if (Utils.SetField(ref _currentProject_bf, project)) {
                NotifyFlag(NotificationFlag.CurrentProject);
            }
        }

        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath)
        {
            using var loadingScope = new LoadingScope(this);

            var pmodel = await ProjectIO.LoadAsync(filePath);
            if (pmodel is null)
                return false;

            var proj = new ProjectEditorModel(pmodel, filePath);
            var clipLoaded = await proj.LoadAudioClipAsync();
            if (!clipLoaded)
                return false;

            SetCurrentProject(proj);
            return true;
        }

        public void UnloadCurrentProject()
        {
            SetCurrentProject(null!);
        }

        public async UniTask SaveCurrentProjectAsync()
        {
            ValidateProject();

            _saveCts.CancelAndReset();
            await SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project));
        }

        public async UniTask SaveCurrentProjectToAsync(string targetFilePath)
        {
            ValidateProject();

            _saveCts.CancelAndReset();
            await SaveCurrentProjectToAsyncInternal(targetFilePath, _saveCts.Token);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project));
        }

        private async UniTask SaveCurrentProjectToAsyncInternal(string targetFilePath, CancellationToken cancellationToken)
        {
            using var scope = new SavingScope(this);

            AssertProjectLoaded();
            await ProjectIO.SaveAsync(CurrentProject.ToModel(), targetFilePath, cancellationToken);
        }

        public async UniTask SaveCurrentProjectChartJsonsAsync()
        {
            ValidateProject();
            await SaveCurrentProjectChartJsonsToAsyncInternal(Path.GetDirectoryName(CurrentProject.ProjectFilePath));
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons));
        }

        public async UniTask SaveCurrentProjectChartJsonsToAsync(string targetDirectory)
        {
            ValidateProject();
            await SaveCurrentProjectChartJsonsToAsyncInternal(targetDirectory);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons));
        }

        private async UniTask SaveCurrentProjectChartJsonsToAsyncInternal(string targetDirectory)
        {
            AssertProjectLoaded();

            _saveChartsCts.CancelAndReset();

            var time = DateTime.Now;
            string dir = Path.Combine(Path.GetDirectoryName(CurrentProject.ProjectFilePath), AutoSaveJsonDirName);
            string filename = Path.GetFileNameWithoutExtension(CurrentProject.ProjectFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var tasks = new Task[CurrentProject.Charts.Count];
            for (int i = 0; i < CurrentProject.Charts.Count; i++) {
                var chart = CurrentProject.Charts[i];
                var chartname = string.IsNullOrEmpty(chart.Name) ? chart.Difficulty.ToLowerCaseString(_environment.GameVersion) : chart.Name;

                tasks[i] = File.WriteAllTextAsync(
                    Path.Combine(dir, $"{filename}.{chartname}.{time:yyMMddHHmmss}.json"),
                    chart.ToData().ToJsonString(), _saveChartsCts.Token);
            }

            await Task.WhenAll(tasks);
        }

        #region Validation

        [MemberNotNull(nameof(CurrentProject))]
        private void ValidateProject()
        {
            if (CurrentProject is null)
                throw new InvalidOperationException("No project loaded.");
        }

        /// <summary>
        /// The method do <c>UnityEngine.Debug.Assert()</c>, and could make IDE
        /// provide a better nullable diagnostic in context
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        [MemberNotNull(nameof(CurrentProject))]
        public void AssertProjectLoaded(string message = "Project not loaded")
#pragma warning disable CS8774
            => Debug.Assert(CurrentProject is not null, message);
#pragma warning restore CS8774

        [MemberNotNullWhen(true, nameof(CurrentProject))]
        public bool IsProjectLoaded() => CurrentProject is not null;

        #endregion

        public enum NotificationFlag
        {
            IsLoading,
            IsSaving,

            AutoSave,
            AutoSaveInterval,

            CurrentProject,
            ProjectAudio,
            ProjectMusicName,
            ProjectComposer,
            ProjectChartDesigner,
            ProjectCharts,
        }

        private readonly struct LoadingScope : IDisposable
        {
            private readonly ProjectManager _self;

            public LoadingScope(ProjectManager self)
            {
                _self = self;
                _self.IsLoading = true;
            }

            public void Dispose()
            {
                _self.IsLoading = false;
            }
        }

        private readonly struct SavingScope : IDisposable
        {
            private readonly ProjectManager _self;

            public SavingScope(ProjectManager self)
            {
                _self = self;
                _self.IsSaving = true;
            }

            public void Dispose()
            {
                _self.IsSaving = false;
            }
        }
    }
}