#nullable enable

using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.Stage
{
    [MovedFrom("Deenote.Core.GameStage")]
    internal sealed class PlacementNotePlaneController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage = default!;
        [SerializeField] PlacementNoteIndicatorController _prefab;

        public GameStageController GameStage => _stage;

        public Transform ContentTransform => transform;
    }
}