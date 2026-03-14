#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class PlacementNotePlaneController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage = default!;
        [SerializeField] PlacementNoteIndicatorController _prefab;

        public GameStageController GameStage => _stage;

        public Transform ContentTransform => transform;
    }
}