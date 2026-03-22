#nullable enable

using Deenote;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.Core.GameStage.Foreground
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ForegroundPerspectiveViewUI : MonoBehaviour
    {
        [field: SerializeField]
        public RectTransform RectTransform { get; private set; } = default!;
        [field: SerializeField]
        public GameStageUIArgs Args { get; private set; } = default!;

        protected abstract void Awake();

        protected virtual void OnValidate()
        {
            RectTransform ??= GetComponent<RectTransform>();
        }
    }
}