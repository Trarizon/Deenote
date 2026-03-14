#nullable enable

using Deenote.Library;
using NaughtyAttributes;
using UnityEngine;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    [RequireComponent(typeof(SpriteRenderer))]
    internal sealed class DeemoGameStageHoldNoteSpriteHandler : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _spriteRenderer = default!;

        // Set this so that we can use transform.z as hold's scale.y
        [Button("Set scale by sprite bounds")]
        private void SetScaleBySprite()
        {
            if (_spriteRenderer != null) {
                this.transform.WithLocalScaleY(1 / _spriteRenderer.sprite.bounds.size.y);
            }
        }

        private void OnValidate()
        {
            _spriteRenderer ??= GetComponent<SpriteRenderer>();
        }
    }
}
