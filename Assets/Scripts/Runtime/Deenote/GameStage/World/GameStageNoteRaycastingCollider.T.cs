using UnityEngine;

namespace Deenote.GameStage.World
{
    [RequireComponent(typeof(Collider))]
    internal abstract class GameStageNoteRaycastingCollider<T> : MonoBehaviour
    {
        public abstract T Note { get; }
    }
}
