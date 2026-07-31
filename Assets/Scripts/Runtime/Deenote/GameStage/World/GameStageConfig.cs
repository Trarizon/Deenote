using UnityEngine;

namespace Deenote.GameStage.World
{
    [CreateAssetMenu(
        fileName = "GameStageConfig",
        menuName = "Deenote/GameStage/GameStageConfig"
    )]
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
    }
}