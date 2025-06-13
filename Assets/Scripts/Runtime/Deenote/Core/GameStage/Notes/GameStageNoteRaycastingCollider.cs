#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage.Notes
{
    [RequireComponent(typeof(BoxCollider))]
    internal sealed class GameStageNoteRaycastingCollider : MonoBehaviour, IGameStageNoteRaycastingCollider
    {
        [SerializeField] BoxCollider _collider;
        [SerializeField] GameStageNoteController _noteController;

        public GameStageNoteController NoteController => _noteController;

        IGameStageNoteController IGameStageNoteRaycastingCollider.NoteController => NoteController;

        private void OnValidate()
        {
            _collider ??= GetComponent<BoxCollider>();
        }
    }
}