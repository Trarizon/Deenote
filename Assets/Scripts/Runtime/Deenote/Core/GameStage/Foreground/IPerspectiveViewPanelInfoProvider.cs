#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.Core.GameStage.Foreground
{
    public interface IPerspectiveViewPanelInfoProvider
    {
        bool IsMouseHovering { get; }
        bool TryConvertScreenPointToViewportPoint(Vector2 screenPoint,out Vector2 viewportPoint);

        event Action<PointerEventData>? PointerDown;
        event Action<PointerEventData>? PointerUp;
        event Action<PointerEventData>? PointerMove;
    }
}
