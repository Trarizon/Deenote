using Deenote.Core.GameStage;
using UnityEngine;

namespace Deenote.GameStage.World
{
    public sealed class GameStageNotePlaneController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage;

        public Transform ContentTransform => transform;

        public Plane Plane => new Plane(transform.up, transform.position);

        public (float X, float Z) Origin => (0f, 0f);
    }
}