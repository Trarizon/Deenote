using Cysharp.Threading.Tasks;
using Deenote.GameStage.Themes;
using Deenote.Scenes;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deenote
{
    partial class App
    {
        private UniTaskCompletionSource<GameStageScene> _gameStageSceneTask = new();

        public async UniTask LoadGameStageSceneAsync()
        {
            var result = await base.LoadSceneAsync<GameStageScene>(GameStageScene.SceneName, LoadSceneMode.Additive);
            _gameStageSceneTask.TrySetResult(result);

            Logger.LogInfo("GameStageScene loaded");
        }

        internal UniTask<GameStageScene> GetGameStageSceneAsync()
        {
            return _gameStageSceneTask.Task;
        }
    }
}
