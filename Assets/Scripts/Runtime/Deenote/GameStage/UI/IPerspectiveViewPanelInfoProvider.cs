#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.GameStage.UI
{
    public interface IPerspectiveViewPanelInfoProvider
    {
        RenderTexture ViewRenderTexture { get; }
        Transform ForegroundParent { get; }
        bool IsMouseHovering { get; }
        bool TryConvertScreenPointToViewportPoint(Vector2 screenPoint,out Vector2 viewportPoint);

        event Action<PointerEventData>? PointerDown;
        event Action<PointerEventData>? PointerUp;
        event Action<PointerEventData>? PointerMove;
        event Action<IPerspectiveViewPanelInfoProvider>? ViewSizeChanged;
    }
}
