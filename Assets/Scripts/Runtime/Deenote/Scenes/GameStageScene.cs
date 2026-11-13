using Deenote.Core.Unity;
using UnityEngine;

namespace Deenote.Scenes
{
    internal sealed class GameStageScene : SceneObject<GameStageScene>
    {
        public const string SceneName = "StageScene";

        [SerializeField] private Transform _stageRoot;
        public Transform StageRoot => _stageRoot;

    }
}