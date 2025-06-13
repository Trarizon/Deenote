#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage.Notes
{
    [RequireComponent(typeof(Collider))]
    internal sealed class GameStageSpeedChangeWarningNoteRaycastingCollider : MonoBehaviour, IGameStageNoteRaycastingCollider
    {
        [SerializeField] Collider _collider;
        [SerializeField] GameStageSpeedChangeWarningNoteController _noteController;

        public GameStageSpeedChangeWarningNoteController NoteController => _noteController;

        IGameStageNoteController IGameStageNoteRaycastingCollider.NoteController => NoteController;

        private void OnValidate()
        {
            _collider ??= GetComponent<BoxCollider>();
        }
    }
}
