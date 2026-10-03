#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.CoreB.Helpers;
using Deenote.CoreB.Helpers;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using Deenote.Library;
using Deenote.Library.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote.Core.Project
{
    [Obsolete]
    public sealed partial class ProjectManager : FlagNotifiableMonoBehaviour<ProjectManager, ProjectManager.NotificationFlag>
    {
        public ProjectModel? CurrentProject => App.ProjectManager.CurrentProject;

        public ChartModel? CurrentChart => MainSystem.GamePlayManager.CurrentChart;

        public AudioClip? AudioClip => App.ProjectManager.CurrentAudioClip;

        private bool _isLoading_bf;
        private bool _isSaving_bf;
        private ResetableCancellationTokenSource _saveCts = new();
        private ResetableCancellationTokenSource _saveChartsCts = new();

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

        public void SetCurrentProject(ProjectModel project, AudioClip audio)
        {
            App.ProjectManager.OpenProject(project, audio);
            NotifyFlag(NotificationFlag.CurrentProject);
        }

        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath)
        {
            return await App.ProjectManager.OpenLoadProjectFileAsync(filePath);
        }

        public void UnloadCurrentProject()
        {
            App.ProjectManager.UnloadCurrentProject();
        }

        public async UniTask SaveCurrentProjectAsync()
        {
            await App.ProjectManager.SaveCurrentProjectAsync();
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project));
        }

        public async UniTask SaveCurrentProjectToAsync(string targetFilePath)
        {
            await App.ProjectManager.SaveCurrentProjectToAsync(targetFilePath);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project));
        }

        public async UniTask SaveCurrentProjectChartJsonsAsync()
        {
            await App.ProjectManager.SaveCurrentProjectChartJsonsToAsync(Path.GetDirectoryName(CurrentProject.ProjectFilePath));
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons));
        }

        public async UniTask SaveCurrentProjectChartJsonsToAsync(string targetDirectory)
        {
            await App.ProjectManager.SaveCurrentProjectChartJsonsToAsync(targetDirectory);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons));
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