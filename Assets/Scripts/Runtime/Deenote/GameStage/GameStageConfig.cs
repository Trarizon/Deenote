#nullable enable

using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage
{
    [MovedFrom("Deenote.Core.GameStage")]
    [CreateAssetMenu(
        fileName = nameof(GameStageConfig),
        menuName = "Deenote/GameStage/GameStageConfig")]
    public sealed class GameStageConfig : ScriptableObject
    {
        public float NotePosToWorldXFactor = 1;
        public float NoteTimeToWorldZFactor = 1;
        public float NoteAppearAheadTimeFactor = 1;

        [Space]
        [Tooltip("This value affects that when a note should be released")]
        public float NoteHitEffectMaxDuration;
        [Range(0f, 1f)]
        public float NoteFadeInRatio;
        [Range(0f, 1f)]
        public float GridLineFadeInRatio;
    }
}