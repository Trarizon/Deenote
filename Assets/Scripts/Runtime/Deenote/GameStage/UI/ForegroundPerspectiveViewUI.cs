#nullable enable

using UnityEngine;

namespace Deenote.GameStage.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ForegroundPerspectiveViewUI : MonoBehaviour
    {
        [field: SerializeField]
        public RectTransform RectTransform { get; private set; } = default!;
        [field: SerializeField]
        public GameStageUIConfig Config { get; private set; } = default!;

        protected abstract void Awake();

        protected virtual void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
        }
    }
}