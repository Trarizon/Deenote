#nullable enable

using CommunityToolkit.Diagnostics;
using Cysharp.Threading.Tasks;
using Deenote.Core.GameStage.Themes;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Deenote.Core.GameStage
{
    public sealed class GameStageManager
    {
        private GameStageThemeConfig[] _themes;

        private SceneInstance? _current;
        private GameStageThemeEntry? _currentEntry;

        private CancellationTokenSource? _cts;

        public GameStageManager()
        {
            _themes = Array.Empty<GameStageThemeConfig>();
        }

        public event Action<GameStageThemeEntry?>? Loaded;
        public event Action? Unloading;

        // 之前是Loader是个Monobehavior，因为始终只有一个所以是单例，
        // 现在看着改，如果是scene的话，应该是GameStageThemeEntry的单例
        // 如果是prefab的话应该直接获取实例化后的
        public async UniTask<GameStageThemeEntry?> LoadAsync(string sceneId, CancellationToken cancellationToken = default)
        {
            if (_cts is not null) {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            var themeConfig = _themes.FirstOrDefault(x => x.Id == sceneId);
            if (themeConfig is null) {
                Debug.LogWarning("Scene not found");
                return null;
            }


            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var ct = _cts.Token;
            SceneInstance scene = default;
            try {
                scene = await Addressables.LoadSceneAsync(themeConfig.SceneReference, LoadSceneMode.Additive)
                    .WithCancellation(cancellationToken);

                if (_current is { } cur) {
                    // Set _currentScene before unload
                    // If cancellation requested after new scene loaded, the _currentScene is the old scene,
                    // and the new ThemeLoad task will unload the old scene
                    // If cancellation requested during or after unload, the _currentScene is the new scene,
                    // we need to prevent repeatly unloading the old scene in other task, so set null
                    _current = null;
                    Unloading?.Invoke();
                    await Addressables.UnloadSceneAsync(cur);
                }

                cancellationToken.ThrowIfCancellationRequested();
                _current = scene;
                _currentEntry = null;
                foreach (var rootObj in scene.Scene.GetRootGameObjects()) {
                    if (rootObj.TryGetComponent<GameStageController>(out var stage)) {
                        _currentEntry = new GameStageThemeEntry(themeConfig, stage);
                        break;
                    }
                }
                if (_currentEntry is null) {
                    Debug.LogError($"Failed to find GameStageController in theme {themeConfig.Name}");
                    ThrowHelper.ThrowInvalidOperationException($"Failed to find GameStageController in theme {themeConfig.Name}");
                }
            } catch (OperationCanceledException) {
                await Addressables.UnloadSceneAsync(scene);
                throw;
            }
            Loaded?.Invoke(_currentEntry);
            return _currentEntry;
        }
    }
}
