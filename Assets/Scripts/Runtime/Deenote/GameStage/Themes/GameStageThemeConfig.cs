using Deenote.Core.Editing;
using Deenote.Core.GameStage;
using Deenote.GameStage.UI;
using TriInspector;
using UnityEngine;

namespace Deenote.GameStage.Themes
{
    [CreateAssetMenu(
        fileName = "GameStageThemeConfig",
        menuName = "Deenote/GameStage/GameStageThemeConfig")]
    internal sealed class GameStageThemeConfig : ScriptableObject
    {
        public string Id = "";
        public string Name = "";

        [Title("Prefabs")]
        public GameStageController StagePrefab;
        public GameStageForegroundView ForegroundPrefab;
        public GameStageNoteController NotePrefab;
        public GameStageSpeedChangeWarningNoteController WarningNotePrefab;
        public PlacementNoteIndicatorController NoteIndicatorPrefab;
        public SpeedChangeWarningIndicatorController WarningNoteIndicatorPrefab;
    }
}
