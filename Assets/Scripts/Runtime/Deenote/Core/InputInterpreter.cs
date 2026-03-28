#nullable enable

using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.CoreB.Models;
using Deenote.InputSystem.InputActions;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core
{
    internal sealed partial class InputInterpreter : MonoBehaviour
    {
        private KeyboardShortcutInputActions _inputActions = default!;

        private GamePlayManager _gamePlay;
        private StageDragSelector _stageDragSelector;

        private bool _dragSelectionToggleMode;

        private void LeftMouseDown(Vector2 screenPoint)
        {
            _mouseInputData.SetPressAt(screenPoint);
            _mouseInputData.LeftMouseDown = true;
            switch (State) {
                case StateFlag.Cancelling:
                    break;
                case StateFlag.Idle:
                    if (TryConvertScreenPointToNoteCoord(screenPoint, false, out var coord)) {
                        _inputCoordData.SetPressAt(coord);
                        State = BeginSelection();
                    }
                    break;
            }
        }

        private StateFlag BeginSelection()
        {
            if (!IsInNoteSelectionArea(_inputCoordData.Coord))
                return State;

            //_prototypes.ForceSetVisibility(false)
            _stageDragSelector.BeginDragSelect(_inputCoordData.Coord, _dragSelectionToggleMode ? StageDragSelectionMode.Toggle : StageDragSelectionMode.Reset);
            return StateFlag.Selecting;
        }

        private bool IsInNoteSelectionArea(NoteCoord coord)
        {
            return true;
        }

        private StateFlag _state_bf;
        private StateFlag State
        {
            get => _state_bf;
            set {
                if (Utils.SetField(ref _state_bf, value)) {
                }
            }
        }

        private enum StateFlag
        {
            Disabled,
            // 双键按下的取消状态，不显示note indicator
            Cancelling,
            // 鼠标未按下、非粘贴
            Idle,
            // 右键按下，放置单个音符
            PlacingSingleNote,
            // 右键按下，放置slide链
            PlacingSlides,
            // 右键按下，放置粘贴内容
            PlacingPasted,
            // 左键按下，框选中
            Selecting,
        }
    }
}
