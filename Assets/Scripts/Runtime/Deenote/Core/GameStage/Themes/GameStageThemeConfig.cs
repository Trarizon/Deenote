#nullable enable

using Deenote.GamePlay.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Deenote.Core.GameStage.Themes
{
    [CreateAssetMenu(
        fileName = "GameStageThemeConfig",
        menuName = "Deenote/GameStageThemeConfig")]
    internal sealed class GameStageThemeConfig : ScriptableObject
    {
        public string Id = "";
        public string Name = "";

        [Header("Prefabs")]
        public AssetReference SceneReference;
        public PerspectiveViewForegroundBase GameUIPrefab;
        public GameStageNoteController NotePrefab;
    }
}
