using System;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.CoreB.Unity.UI
{
    [MovedFrom("Deenote.Library.Unity.UI")]
    public sealed class PointerHoveringTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [ReadOnly, ShowInInspector]
        public bool IsHovering { get; private set; }

        public event Action<PointerHoveringTrigger, bool>? IsHoveringChanged;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsHovering = true;
            IsHoveringChanged?.Invoke(this, true);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsHovering = false;
            IsHoveringChanged?.Invoke(this, false);
        }
    }
}