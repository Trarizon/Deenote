#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Contexts;
using Deenote.Core.Project;
using Deenote.CoreB.Notification;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.ProjectManagement
{
    [MovedFrom("Deenote")]
    public sealed class AutoSaveTrigger : MonoBehaviour
    {
        private const string AutoSaveJsonDirName = $"Deenote_AutoSave";

        private ProjectManagerB _projectManager;
        private EnvironmentContext _environment;

        private float _timer;

        public bool IsEnabled
        {
            get => enabled;
            set {
                enabled = value;
                if (value)
                    _ = AutoSaveProjectAsync();
            }
        }

        public event Action? Saving;
        public event Action? Saved;

        private void Awake()
        {
            _projectManager = MainSystem.ProjectManagerB;
            _environment = MainSystem.Contexts.Environment;
        }

        private void Start()
        {
            _environment.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.AutoSave))) {
                    enabled = s.AutoSave is not ProjectAutoSaveOption.Off;
                }
            });
        }

        private void OnEnable()
        {
            AutoSaveProjectAsync().Forget();
        }

        private void OnDisable()
        {
            _timer = 0f;
        }

        private void Update()
        {
            if (MathUtils.IncAndTryWrap(ref _timer, Time.unscaledDeltaTime, _environment.AutoSaveIntervalTime)) {
                _ = AutoSaveProjectAsync();
            }
        }

        private async UniTask AutoSaveProjectAsync()
        {
            if (!MainSystem.ProjectManager.IsProjectLoaded())
                return;
            if (_projectManager.SavingStatus is ProjectSavingStatus.Saving)
                return;
            if (!MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange)
                return;

            switch (_environment.AutoSave) {
                case ProjectAutoSaveOption.On:
                    Saving?.Invoke();
                    await _projectManager.SaveCurrentProjectAsync();
                    Saved?.Invoke();
                    return;
                case ProjectAutoSaveOption.OnAndSaveJson:
                    Saving?.Invoke();
                    var proj = _projectManager.SaveCurrentProjectAsync();
                    var dir = Path.Combine(Path.GetDirectoryName(MainSystem.ProjectManager.CurrentProject.ProjectFilePath), AutoSaveJsonDirName);
                    var charts = _projectManager.SaveCurrentProjectChartJsonsToAsync(dir);
                    await proj;
                    await charts;
                    Saved?.Invoke();
                    return;
                default:
                    break;
            }
        }
    }
}