#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    internal sealed class DeemoV4GameStageNoteEffect : DeemoGameStageNoteEffect
    {
        [SerializeField] Transform _glowScaler = default!;

        public override void SetNoteSizeScaler(float value)
        {
            _explosionScaler.localScale = new Vector3(value, 1, 1);
            _circlewaveScaler.localScale = new Vector3(value, 1, 1);
            _shockwaveScaler.localScale = new Vector3(value, 1, 1);
            _holdingExplosionScaler.localScale = new Vector3(value, 1, 1);
            _glowScaler.localScale = new Vector3(value, 1, 1);
        }
    }
}
