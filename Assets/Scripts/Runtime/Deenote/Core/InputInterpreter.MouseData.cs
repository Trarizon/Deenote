#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.Core
{
    partial class InputInterpreter : MonoBehaviour
    {
        private MouseInputData _mouseInputData = new();
        private InputCoordData _inputCoordData = new();
        private NoteCoord _quantizedPressedCoord = new();

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint,bool applyHighlightSpeed,out NoteCoord coord)
        {
            if(_gamePlay.Stage is null) {
                coord = default;
                return false;
            }

            coord = default;
            return true;
        }

        private struct MouseInputData
        {
            public bool LeftMouseDown;
            public bool RightMouseDown;
            public Vector2 ScreenPoint { get; private set; }
            public Vector2 PressedScreenPoint { get; private set; }
            public readonly Vector2 DraggedDelta => ScreenPoint - PressedScreenPoint;

            public void SetPressAt(Vector2 screenPoint)
            {
                PressedScreenPoint = screenPoint;
                ScreenPoint = screenPoint;
            }

            public void SetMove(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
            }

            public void SetRelease(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
#if UNITY_EDITOR
                // To make it crush in editor
                PressedScreenPoint = new(float.NaN, float.NaN);
#endif
            }
        }

        private struct InputCoordData
        {
            public NoteCoord PressedCoord { get; private set; }
            public NoteCoord PrevCoord { get; private set; }
            public NoteCoord Coord { get; private set; }
            public readonly NoteCoord DraggedDelta => Coord - PressedCoord;

            public void SetPressAt(NoteCoord coord)
            {
                PressedCoord = coord;
                Coord = coord;
#if UNITY_EDITOR
                PrevCoord = new NoteCoord(float.NaN, float.NaN);
#endif
            }

            public void SetMove(NoteCoord coord)
            {
                PrevCoord = Coord;
                Coord = coord;
            }

            public void SetRelease(NoteCoord coord)
            {
                PrevCoord = Coord;
                Coord = coord;
#if UNITY_EDITOR
                PressedCoord = new(float.NaN, float.NaN);
#endif
            }

            public void ShiftCoordTime(float delta)
            {
                Coord = Coord with { Time = Coord.Time + delta };
            }
        }
    }
}
