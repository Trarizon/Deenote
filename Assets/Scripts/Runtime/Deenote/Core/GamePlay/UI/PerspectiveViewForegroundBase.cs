#nullable enable

using Deenote.GameStage.UI;
using UnityEngine;

namespace Deenote.GamePlay.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class PerspectiveViewForegroundBase : GameStageForegroundView
    {
        [field: SerializeField]
        public RectTransform RectTransform { get; private set; } = default!;

        protected abstract void Awake();

        protected virtual void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
        }
    }
}