using Deenote.Core.GameStage;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.World
{
    [MovedFrom("Deenote.Core.GameStage")]
    internal sealed class GameStageNoteRaycastingCollider : GameStageNoteRaycastingCollider<GameStageNoteController>
    {
        [SerializeField] GameStageNoteController _noteController;

        public override GameStageNoteController Note => _noteController;

        private void OnValidate()
        {
            _noteController ??= GetComponentInParent<GameStageNoteController>();
        }
    }
}