using Deenote.GameStage.World;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class SpeedChangeWarningRaycastingCollider : GameStageNoteRaycastingCollider<GameStageSpeedChangeWarningNoteController>
    {
        [SerializeField] private GameStageSpeedChangeWarningNoteController _warningController = default!;

        public override GameStageSpeedChangeWarningNoteController Note => _warningController;

        private void OnValidate()
        {
            _warningController ??= GetComponentInParent<GameStageSpeedChangeWarningNoteController>();
        }
    }
}
