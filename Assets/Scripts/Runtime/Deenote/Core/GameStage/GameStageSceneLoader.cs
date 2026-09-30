#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.GamePlay.UI;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deenote.Core.GameStage
{
    public sealed class GameStageSceneLoader : MonoBehaviour
    {
        private static Scene? _loadedStageScene;
        private static GameStageSceneLoader? _instance;

        public static event Action<GameStageSceneLoader>? StageLoaded;

        [field: SerializeField]
        public GameStageController StageController { get; private set; } = default!;

        [field: SerializeField]
        public PerspectiveViewForegroundBase PerspectiveViewForeground { get; private set; } = default!;

        private void Awake()
        {
            _instance = this;
        }

        public static async UniTask<GameStageSceneLoader> LoadAsync(string scene)
        {
            App.Logger.LogDebug($"LoadAsync {scene}");
            var prevLoadedScene = _loadedStageScene;
            var loadOp = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            loadOp.completed += _ => { Debug.Log("AsyncOperation.completed"); };
            var utcs = new UniTaskCompletionSource();
            SceneManager.sceneLoaded += (loadedScene, mode) =>
            {
                App.Logger.LogDebug($"sceneLoaded: {loadedScene.name}");
                utcs.TrySetResult();
                App.Logger.LogDebug($"sceneLoaded {loadedScene.name} 2");
            };
            await UniTask.WaitUntil(() => loadOp.isDone);
            Debug.Log("isDone!");
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                App.Logger.LogDebug($"sceneLoaded {scene.name} 1");
                _loadedStageScene = scene;
            };
            await UniTask.Yield();
            App.Logger.LogDebug($"After sceneLoaded: isDone={loadOp.isDone}, progress={loadOp.progress}, allowSceneActivation={loadOp.allowSceneActivation}");

            async UniTaskVoid Await()
            {
                await loadOp;
            }
            // Await();

            // utcs.Task.GetAwaiter().OnCompleted(() => Debug.Log("after"));

        await utcs.Task;

            Debug.Log("after");
            // var dele=(loadOp.GetType().GetField("m_completeCallback").GetValue(loadOp) as Delegate).GetInvocationList().Length;

            // Debug.Log($"m_completeCallback: {dele}");

            var awaiter = loadOp.GetAwaiter();

            if (prevLoadedScene is { } loadedScene) {
                _ = SceneManager.UnloadSceneAsync(loadedScene);
            }

            StageLoaded?.Invoke(_instance!);
            App.Logger.LogDebug($"StageLoaded {scene}");
            Debug.Assert(_instance != null);
            return _instance!;
        }
    }
}