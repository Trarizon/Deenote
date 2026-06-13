#nullable enable

using Deenote.GameStage.UI;
using Deenote.InputSystem.InputActions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deenote.Editing
{
    internal sealed class InputInterpreter : MonoBehaviour
    {
        private KeyboardShortcutInputActions _inputActions;

        private MouseEditingCoordinator _mouseEditingCoordinator;
        private IPerspectiveViewPanelInfoProvider _perspectiveViewPanelInfoProvider;

        public KeyboardShortcutInputActions InputActions => _inputActions;

        private void Awake()
        {
            _inputActions = new();

            _mouseEditingCoordinator = MainSystem.MouseEditingCoordinator;
            _perspectiveViewPanelInfoProvider = MainSystem.PerspectiveViewPanelInfo;
        }

        // private void Start()
        // {
        //     UISystem.FocusedControlChanged += ctrl =>
        //     {
        //         SetGeneralsEnable(ctrl is null);
        //     };
        // }

        private void OnEnable()
        {
            EnableActions();
        }

        private void OnDisable()
        {
            DisableActions();
        }

        private void Update()
        {
            Update_MouseAction();
        }

        private void Update_MouseAction()
        {
            var mouse = Mouse.current;
            var pos = mouse.position.ReadValue();

            if (_perspectiveViewPanelInfoProvider.IsMouseHovering) {
                if (mouse.leftButton.wasPressedThisFrame)
                    _mouseEditingCoordinator.LeftMouseDown(pos);
                if (mouse.leftButton.wasReleasedThisFrame)
                    _mouseEditingCoordinator.LeftMouseUp(pos);
                if (mouse.rightButton.wasPressedThisFrame)
                    _mouseEditingCoordinator.RightMouseDown(pos);
                if (mouse.rightButton.wasReleasedThisFrame)
                    _mouseEditingCoordinator.RightMouseUp(pos);
                _mouseEditingCoordinator.MouseMove(pos);
            }
            else {
                _mouseEditingCoordinator.MouseMoveOutOfRange();
            }
        }

        private void EnableActions()
        {
            _inputActions.Enable();
        }

        private void DisableActions()
        {

        }
    }
}
