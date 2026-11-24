using Deenote.Inputting;
using System;
using UnityEngine.EventSystems;

namespace Deenote.UI.Views
{
    partial class PerspectiveViewPanelView : IMouseInputHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        public event Action<MouseButtonEventArgs>? Pressed;
        public event Action<MouseButtonEventArgs>? Released;
        public event Action<MouseMovedEventArgs>? Moved;

        public void OnPointerDown(PointerEventData eventData)
        {
            Pressed?.Invoke(new MouseButtonEventArgs(eventData));
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            Released?.Invoke(new MouseButtonEventArgs(eventData));
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            Moved?.Invoke(new MouseMovedEventArgs(eventData));
        }
    }
}