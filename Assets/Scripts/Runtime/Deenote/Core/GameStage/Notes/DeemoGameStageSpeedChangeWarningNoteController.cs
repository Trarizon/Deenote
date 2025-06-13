#nullable enable

using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage.Notes
{
    internal sealed class DeemoGameStageSpeedChangeWarningNoteController : GameStageSpeedChangeWarningNoteController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer;

        protected override void SetNoteSpriteColorRGB(Color color)
        {
            _noteSpriteRenderer.WithColorRGB(color);
        }
    }
}
