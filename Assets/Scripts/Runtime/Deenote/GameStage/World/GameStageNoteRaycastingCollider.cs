#nullable enable

using Deenote.Core.GameStage;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.World
{
    [MovedFrom("Deenote.Core.GameStage")]
    [RequireComponent(typeof(Collider))]
    internal sealed class GameStageNoteRaycastingCollider : MonoBehaviour
    {
        [SerializeField] Collider _collider;
        [SerializeField] GameStageNoteController _noteController;

        public GameStageNoteController NoteController => _noteController;

        private void OnValidate()
        {
            _collider ??= GetComponent<Collider>();
            _noteController ??= GetComponentInParent<GameStageNoteController>();
        }
    }
}