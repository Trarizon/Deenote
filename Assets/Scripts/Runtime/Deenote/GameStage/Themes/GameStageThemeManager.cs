using Cysharp.Threading.Tasks;
using Deenote.Core.Unity;
using Deenote.CoreB.Notification;
using Deenote.GameStage.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Deenote.GameStage.Themes
{
    public sealed class GameStageThemeManager : INotifyPropertyChanged<GameStageThemeManager>
    {
        public event Action<GameStageThemeManager, PropertyChangedEventArgs>? PropertyChanged;

        private readonly GameStageThemeConfig _defaultTheme;
        private readonly Dictionary<string, GameStageThemeConfig> _themes = new();

        private UniTask<Transform> _sceneStageRootTask;

        private GameStageThemeConfig? _currentThemeConfig;

        internal GameStageThemeConfig? CurrentTheme
        {
            get => _currentThemeConfig;
            set {
                if (_currentThemeConfig != value) {
                    _currentThemeConfig = value;
                    PropertyChanged?.Invoke(this, new(nameof(CurrentTheme)));
                }
            }
        }

        private GameStageController? _currentStageInstance;
        public GameStageController? CurrentStage
        {
            get => _currentStageInstance;
            set {
                if (_currentStageInstance != value) {
                    _currentStageInstance = value;
                    PropertyChanged?.Invoke(this, new(nameof(CurrentStage)));
                }
            }
        }

        private GameStageForegroundView? _currentForegroundInstance;
        public GameStageForegroundView? CurrentForeground
        {
            get => _currentForegroundInstance;
            set {
                if (_currentForegroundInstance != value) {
                    _currentForegroundInstance = value;
                    PropertyChanged?.Invoke(this, new(nameof(CurrentForeground)));
                }
            }
        }

        internal GameStageThemeManager(GameStageThemeConfig[] builtinThemes)
        {
            _defaultTheme = builtinThemes[0];
            foreach (var theme in builtinThemes) {
                _themes.Add(theme.Id, theme);
            }

            _sceneStageRootTask = App.Current.GetGameStageSceneAsync()
                .ContinueWith(scene => scene.StageRoot)
                .Preserve();
        }

        public async UniTask<bool> TryLoadThemeAsync(string themeId, CancellationToken cancellationToken = default)
        {
            if (!_themes.TryGetValue(themeId, out var config))
                return false;

            GameStageController? stage = null;
            try {
                var stageTask = LoadThemeStageAsync(config, cancellationToken);

                stage = await stageTask;
            } catch {
                if (stage != null) {
                    UnityEngine.Object.Destroy(stage);
                }
                App.Logger.LogError($"Failed to load stage theme: {config.Name}");
                throw;
            }

            if (CurrentStage != null) {
                UnityEngine.Object.Destroy(CurrentStage);
            }
            CurrentStage = stage;
            _currentThemeConfig = config;
            CurrentForeground = null;

            return true;
        }

        public async UniTask<GameStageForegroundView?> LoadCurrentThemeForegroundAsync(Transform parent, CancellationToken cancellationToken = default)
        {
            if (_currentThemeConfig is null) {
                App.Logger.LogWarning("No theme loaded, cannot load foreground");
                return null;
            }

            GameStageForegroundView? foreground = null;
            try {
                foreground = await LoadThemeForegroundAsync(_currentThemeConfig, parent, cancellationToken);
            } catch {
                if (foreground != null) {
                    UnityEngine.Object.Destroy(foreground);
                }
                App.Logger.LogError($"Failed to load foreground theme: {_currentThemeConfig.Name}");
                throw;
            }
            if (CurrentForeground != null) {
                UnityEngine.Object.Destroy(CurrentForeground);
            }
            CurrentForeground = foreground;
            return foreground;
        }

        private async UniTask<GameStageController> LoadThemeStageAsync(GameStageThemeConfig config, CancellationToken cancellationToken = default)
        {
            var instances = await UnityEngine.Object.InstantiateAsync(config.StagePrefab, await _sceneStageRootTask)
                .AttachCancellation(cancellationToken)
                .ToUniTask(cancellationToken: cancellationToken);
            var ins = instances[0];
            return ins;
        }

        private async UniTask<GameStageForegroundView> LoadThemeForegroundAsync(GameStageThemeConfig config, Transform parent, CancellationToken cancellationToken = default)
        {
            var instances = await UnityEngine.Object.InstantiateAsync(config.ForegroundPrefab, parent)
                .AttachCancellation(cancellationToken)
                .ToUniTask(cancellationToken: cancellationToken);
            var ins = instances[0];
            return ins;
        }
    }
}
