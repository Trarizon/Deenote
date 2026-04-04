#nullable enable

using Deenote.Core.GameStage;
using Deenote.Core.GameStage.Foreground;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Deenote.GameStage.Themes
{
    [CreateAssetMenu(
        fileName = "GameStageThemeConfig",
        menuName = "Deenote/GameStage/GameStageThemeConfig")]
    internal sealed class GameStageThemeConfig : ScriptableObject
    {
        public string Id = "";
        public string Name = "";

        public GameStageConfig GameStageConfig;
        [Header("Prefabs")]
        public AssetReference SceneReference;
        public ForegroundPerspectiveViewUI GameUIPrefab;
        public GameStageNoteController NotePrefab;
    }
}
