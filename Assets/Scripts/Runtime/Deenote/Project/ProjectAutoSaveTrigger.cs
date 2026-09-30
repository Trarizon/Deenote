using Cysharp.Threading.Tasks;
using Deenote.Library.Mathematics;
using Deenote.Systems;
using System;
using System.IO;

namespace Deenote.Project
{
    public sealed partial class ProjectAutoSaveTrigger
    {
        private const string AutoSaveJsonDirName = $"Deenote_AutoSave";

        private bool _enabled = true;
        private float _timer;

        public bool Enabled
        {
            get => _enabled;
            set {
                if (_enabled != value) {
                    _enabled = value;
                    if (value) {
                        AutoSaveProjectAsync().Forget();
                    }
                    else {
                        _timer = 0f;
                    }
                }
            }
        }

        public event Action<(DateTime Time, AutoSaveOptions Options)>? AutoSaving;
        public event Action<(DateTime Time, AutoSaveOptions Options)>? AutoSaved;

        public ProjectAutoSaveTrigger()
        {
            App.Current.UnscaledTick += delta =>
            {
                if (Enabled) {
                    if (MathUtils.IncAndTryWrap(ref _timer, delta, App.Environment.AutoSaveIntervalSeconds)) {
                        AutoSaveProjectAsync().Forget();
                    }
                }
            };
        }

        private async UniTask AutoSaveProjectAsync()
        {
            if (App.ProjectManager.CurrentProject is null)
                return;
            if (App.ProjectManager.IsProjectSaving)
                return;
            if (!MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange)
                return;

            var options = App.Environment.AutoSaveOptions;
            switch (options) {
                case AutoSaveOptions.Project:
                    AutoSaving?.Invoke((DateTime.Now, options));
                    await App.ProjectManager.SaveCurrentProjectAsync();
                    AutoSaved?.Invoke((DateTime.Now, options));
                    break;
                case AutoSaveOptions.All:
                    AutoSaving?.Invoke((DateTime.Now, options));
                    var tProj = App.ProjectManager.SaveCurrentProjectAsync();
                    var dir = Path.Combine(Path.GetDirectoryName(App.ProjectManager.ProjectFilePath), AutoSaveJsonDirName);
                    var tCharts = App.ProjectManager.SaveCurrentProjectChartJsonsToAsync(dir);
                    await UniTask.WhenAll(tProj, tCharts);
                    AutoSaved?.Invoke((DateTime.Now, options));
                    break;
            }
        }
    }
}