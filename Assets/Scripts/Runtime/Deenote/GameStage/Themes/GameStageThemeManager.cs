#nullable enable

using CommunityToolkit.Diagnostics;
using Cysharp.Threading.Tasks;
using Deenote.Core.GameStage;
using Deenote.Library;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Deenote.GameStage.Themes
{
    public sealed class GameStageThemeManager
    {
        private const string ConfigAssetTag = "GameStageThemeConfig";
        private const string DefaultThemeId = "Deemo";

        private readonly GameStageThemeContext _context;
        private GameStageThemeConfig[]? _themeConfigs;

        private string _idToLoad = DefaultThemeId;

        private SceneInstance? _currentScene;

        public event Action? ThemeUnloading;
        public event Action<GameStageThemeEntry>? ThemeLoaded;

        public GameStageThemeManager(GameStageThemeContext context)
        {
            _context = context;

            InitializeThemeConfigsAsync()
                .ContinueWith(() => LoadThemeAsync(_idToLoad))
                .Forget();

        }

        private async UniTask<bool> LoadThemeAsync(string id)
        {
            foreach (var config in _themeConfigs) {
                if (config.Id == id) {
                    await LoadThemeAsync(config);
                    return true;
                }
            }
            return false;
        }

        private readonly ResettableCancellationTokenSource _loadCts = new();

        private async UniTask LoadThemeAsync(GameStageThemeConfig config)
        {
            var token = _loadCts.ResetAndGetToken();
            SceneInstance scene = default;
            GameStageThemeEntry entry;
            try {
                scene = await config.SceneReference.LoadSceneAsync(LoadSceneMode.Additive).WithCancellation(token);

                GameStageController? stage = null;
                foreach (var rootObj in scene.Scene.GetRootGameObjects()) {
                    if (rootObj.TryGetComponent<GameStageController>(out var controller)) {
                        stage = controller;
                        break;
                    }
                }
                if (stage is null) {
                    ThrowHelper.ThrowInvalidOperationException($"Failed to find GameStageController in scene {scene.Scene.name}");
                }

                entry = new GameStageThemeEntry(config, stage);
                stage.OnInstantiate(entry);

                if (_currentScene is { } prev) {
                    // Set _currentScene before unload
                    // If cancellation requested after new scene loaded, the _currentScene is the old scene,
                    // and the new ThemeLoad task will unload the old scene
                    // If cancellation requested during or after unload, the _currentScene is the new scene,
                    // we need to prevent repeatly unloading the old scene in other task, so set null
                    _currentScene = null;

                    ThemeUnloading?.Invoke();
                    Addressables.UnloadSceneAsync(prev).ToUniTask().Forget();
                }

                _currentScene = scene;
                _context.CurrentTheme = entry;
            } catch (OperationCanceledException) {
                Addressables.UnloadSceneAsync(scene).ToUniTask().Forget();
                throw;
            }
            Debug.Log($"Loaded theme {config.Name}");
            ThemeLoaded?.Invoke(entry);

        }

        private async UniTask InitializeThemeConfigsAsync()
        {
#if UNITY_EDITOR
            try {
#endif
                var themes = await Addressables.LoadAssetsAsync<GameStageThemeConfig>(ConfigAssetTag, null!);
                _themeConfigs = themes.OrderBy(t => t.Id).ToArray();
#if UNITY_EDITOR
            } catch {
                Debug.LogError($"Failed to load stage themes");
                throw;
            }
#endif
            Debug.Log($"Loaded {_themeConfigs.Length} stage themes: {string.Join('\n', _themeConfigs.Select(t => t.Name))}");
        }
    }
}
