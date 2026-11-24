using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.Inputting
{
    public interface IMouseInputHandler
    {
        bool IsHovering { get; }
        event Action<MouseButtonEventArgs>? Pressed;
        event Action<MouseButtonEventArgs>? Released;
        event Action<MouseMovedEventArgs>? Moved;
    }

    public readonly struct MouseButtonEventArgs
    {
        private readonly PointerEventData _eventData;
        public Vector2 ScreenPoint => _eventData.position;
        public bool LeftMouse => _eventData.button == PointerEventData.InputButton.Left;
        public bool RightMouse => _eventData.button == PointerEventData.InputButton.Right;

        public MouseButtonEventArgs(PointerEventData eventData)
        {
            _eventData = eventData;
        }
    }

    public readonly struct MouseMovedEventArgs
    {
        private readonly PointerEventData _eventData;
        public Vector2 ScreenPoint => _eventData.position;

        public MouseMovedEventArgs(PointerEventData eventData)
        {
            _eventData = eventData;
        }
    }
}
