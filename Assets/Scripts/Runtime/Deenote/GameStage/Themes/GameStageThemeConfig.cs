#nullable enable

using Deenote.GameStage.Grids;
using Deenote.GameStage.Stage;
using Deenote.GameStage.UI;
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
        public GridLineConfig GridLineConfig;
        [Header("Prefabs")]
        public AssetReference SceneReference;
        public ForegroundPerspectiveViewUI GameUIPrefab;
        public GameStageNoteController NotePrefab;
        public PlacementNoteIndicatorController NoteIndicatorPrefab;
    }
}
