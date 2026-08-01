using Deenote.Core.Editing;
using Deenote.Core.GameStage;
using TriInspector;
using UnityEngine;

namespace Deenote.GameStage.World
{
    [CreateAssetMenu(
        fileName = "GameStageConfig",
        menuName = "Deenote/GameStage/GameStageConfig"
    )]
    [DeclareBoxGroup("Prefabs")]
    public sealed class GameStageConfig : ScriptableObject
    {
        public float NotePosToWorldXFactor = 1;
        public float NoteTimeToWorldZFactor = 1;
        public float NoteAppearAheadTimeFactor = 30;

        [Space]
        [Tooltip("This value affects that when a note should be released")]
        public float NoteHitEffectMaxDuration;
        [Range(0f, 1f)]
        public float NoteFadeInRatio;
        [Range(0f, 1f)]
        public float GridLineFadeInRatio;

        [Group("Prefabs")][SerializeField] internal GameStageNoteController NotePrefab;
        [Group("Prefabs")][SerializeField] internal PlacementNoteIndicatorController NoteIndicatorPrefab;
    }
}