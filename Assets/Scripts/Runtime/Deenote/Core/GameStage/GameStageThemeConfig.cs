#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage
{
    [CreateAssetMenu(
        fileName = "GameStageThemeConfig",
        menuName = "Deenote/GameStageThemeConfig")]
    internal sealed class GameStageThemeConfig : ScriptableObject
    {
        public string Id = "";
        public string Name = "";
        [Header("Prefabs")]
        public GameStageNoteController NotePrefab;
    }
}
