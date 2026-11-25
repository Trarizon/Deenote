using Deenote.Entities;
using UnityEngine;

namespace Deenote.Inputting
{
    partial class PlacementCoordinator
    {
        private enum FsmState
        {
            Invalid = 0,
            IdleEntry,

            Aborted = FsmSuperState.Aborted,

            IdleSingle = FsmSuperState.Placing,
            IdleSlides,
            IdlePaste,
            IdleWarning,
            PlacingSingle,
            PlacingSlides,
            PlacingPaste,
            PlacingWarning,
            Placed,

            Selecting = FsmSuperState.Selecting,
            Selected,
        }

        private const FsmState SuperStateMask = (FsmState)0xF0;
        private enum FsmSuperState
        {
            Aborted = 0x10,
            Placing = 0x20,
            Selecting = 0x30,
        }

        private FsmState _state_bf;
        private FsmState State
        {
            get => _state_bf;
            set {
                static FsmSuperState SuperStateOf(FsmState state)
                    => (FsmSuperState)(state & SuperStateMask);

                if (_state_bf == value)
                    return;

                App.Logger.LogInfo($"Exiting state {_state_bf}");
                switch (_state_bf) {
                    case FsmState.Selecting: ExitSelecting(); break;
                    default: break;
                }

                var oldSuper = SuperStateOf(_state_bf);
                var newSuper = SuperStateOf(value);
                if (oldSuper != newSuper) {
                    App.Logger.LogInfo($"Exiting super state {oldSuper}");
                    switch (oldSuper) {
                        case FsmSuperState.Placing: ExitSuperPlacing(); break;
                        case FsmSuperState.Selecting: ExitSuperSelecting(); break;
                        default: break;
                    }
                }

                _state_bf = value;

                if (oldSuper != newSuper) {
                    App.Logger.LogInfo($"Entering super state {newSuper}");
                    switch (newSuper) {
                        case FsmSuperState.Placing: EnterSuperPlacing(); break;
                        case FsmSuperState.Selecting: EnterSuperSelecting(); break;
                        default: break;
                    }
                }

                App.Logger.LogInfo($"Entering state {_state_bf}");
                switch (value) {
                    case FsmState.IdleEntry: EnterIdleEntry(); break;
                    case FsmState.IdleSingle: EnterIdleSingle(); break;
                    case FsmState.IdleSlides: EnterIdleSlides(); break;
                    case FsmState.IdlePaste: EnterIdlePaste(); break;
                    case FsmState.IdleWarning: EnterIdleWarning(); break;
                    case FsmState.PlacingSingle: EnterPlacingSingle(); break;
                    case FsmState.PlacingSlides: EnterPlacingSlides(); break;
                    case FsmState.PlacingPaste: EnterPlacingPaste(); break;
                    case FsmState.PlacingWarning: EnterPlacingWarning(); break;
                    case FsmState.Placed: EnterPlaced(); break;

                    case FsmState.Selecting: EnterSelecting(); break;
                    case FsmState.Selected: EnterSelected(); break;
                    default: break;
                }
            }
        }

        private void Process()
        {
            switch (State) {
                case FsmState.IdleSingle:
                    ProcessIdleSingle();
                    break;
                case FsmState.IdleSlides:
                    ProcessIdleSlides();
                    break;
                case FsmState.IdlePaste:
                    ProcessIdlePaste();
                    break;
                case FsmState.PlacingSingle:
                    ProcessPlacingSingle();
                    break;
                case FsmState.PlacingSlides:
                    ProcessPlacingSlides();
                    break;
                case FsmState.Selecting:
                    ProcessSelecting();
                    break;
                default: break;
            }
        }

        #region Definitions

        partial void EnterIdleEntry();

        // Place

        partial void EnterIdleSingle();
        partial void ProcessIdleSingle();
        partial void ExitIdleSingle();

        partial void EnterIdleSlides();
        partial void ProcessIdleSlides();
        partial void ExitIdleSlides();

        partial void EnterIdlePaste();
        partial void ProcessIdlePaste();
        partial void ExitIdlePaste();

        partial void EnterIdleWarning();
        partial void ProcessIdleWarning();
        partial void ExitIdleWarning();

        partial void EnterPlacingSingle();
        partial void ProcessPlacingSingle();
        partial void ExitPlacingSingle();

        partial void EnterPlacingSlides();
        partial void ProcessPlacingSlides();
        partial void ExitPlacingSlides();

        partial void EnterPlacingPaste();
        partial void ProcessPlacingPaste();
        partial void ExitPlacingPaste();

        partial void EnterPlacingWarning();
        partial void ProcessPlacingWarning();
        partial void ExitPlacingWarning();

        partial void EnterPlaced();
        partial void ProcessPlaced();
        partial void ExitPlaced();

        // Select

        partial void EnterSelecting();
        partial void ProcessSelecting();
        partial void ExitSelecting();

        partial void EnterSelected();

        partial void EnterSuperSelecting();
        partial void ExitSuperSelecting();

        partial void EnterSuperPlacing();
        partial void ExitSuperPlacing();

        #endregion

        // Transforms

        private InputCoordData _coordData;
        private MouseInputData _mouseData;

        public void SetEnabled(bool enabled)
        {
            State = enabled ? FsmState.IdleEntry : FsmState.Invalid;
        }

        public void Tick()
        {
            Process();
        }

        public void LeftMouseDown(Vector2 screenPoint)
        {
            _mouseData.PressAt(screenPoint);
            _mouseData.LeftMouseDown = true;

            switch (State) {
                case FsmState.IdleSingle:
                case FsmState.IdleSlides:
                case FsmState.IdlePaste:
                case FsmState.IdleWarning:
                    if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, false, out var coord))
                        return;
                    if (!IsInNoteSelectionArea(coord))
                        return;
                    _coordData.PressAt(coord);
                    State = FsmState.Selecting;
                    break;
                case FsmState.PlacingSingle:
                case FsmState.PlacingSlides:
                case FsmState.PlacingPaste:
                case FsmState.PlacingWarning:
                    State = FsmState.Aborted;
                    break;
            }
        }

        public void LeftMouseUp(Vector2 screenPoint)
        {
            _mouseData.ReleaseAt(screenPoint);
            _mouseData.LeftMouseDown = false;
            switch (State) {
                case FsmState.Selecting:
                    State = FsmState.Selected;
                    break;
                case FsmState.Aborted:
                    if (_mouseData.LeftMouseDown || _mouseData.RightMouseDown)
                        return;
                    State = FsmState.IdleEntry;
                    break;
            }
        }

        public void RightMouseDown(Vector2 screenPoint)
        {
            _mouseData.PressAt(screenPoint);
            _mouseData.RightMouseDown = true;
            switch (State) {
                case FsmState.IdleSingle:
                    if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord))
                        return;
                    if (!IsInNoteSelectionArea(coord))
                        return;
                    _coordData.PressAt(coord);
                    State = FsmState.PlacingSingle;
                    break;
                case FsmState.IdleSlides:
                    if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out coord))
                        return;
                    if (!IsInNotePlacementArea(coord))
                        return;
                    _coordData.PressAt(coord);
                    State = FsmState.PlacingSlides;
                    break;
                case FsmState.IdlePaste:
                    if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out coord))
                        return;
                    if (!IsInNotePlacementArea(coord))
                        return;
                    _coordData.PressAt(coord);
                    State = FsmState.PlacingPaste;
                    break;
                case FsmState.IdleWarning:
                    if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out coord))
                        return;
                    if (!IsInNotePlacementArea(coord))
                        return;
                    _coordData.PressAt(coord);
                    State = FsmState.PlacingWarning;
                    break;
                case FsmState.Selecting:
                    State = FsmState.Selected;
                    break;
            }
        }

        public void RightMouseUp(Vector2 screenPoint)
        {
            _mouseData.ReleaseAt(screenPoint);
            _mouseData.RightMouseDown = false;
            switch (State) {
                case FsmState.PlacingSingle:
                case FsmState.PlacingSlides:
                case FsmState.PlacingPaste:
                case FsmState.PlacingWarning:
                    State = FsmState.Placed;
                    break;
                case FsmState.Aborted:
                    if (_mouseData.LeftMouseDown || _mouseData.RightMouseDown)
                        return;
                    State = FsmState.IdleEntry;
                    break;
            }
        }

        public void MouseMove(Vector2 screenPoint)
        {
            _mouseData.SetPoint(screenPoint);
            Process();
        }

        public void PreparePaste()
        {
            switch (State) {
                case FsmState.IdleSingle:
                case FsmState.IdleSlides:
                case FsmState.IdleWarning:
                    State = FsmState.IdlePaste;
                    break;
            }
        }

        private struct MouseInputData
        {
            public bool InRange;
            public bool LeftMouseDown;
            public bool RightMouseDown;
            public Vector2 ScreenPoint { get; private set; }
            public Vector2 PressedScreenPoint { get; private set; }
            public readonly Vector2 DraggedDelta => ScreenPoint - PressedScreenPoint;

            public void PressAt(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
                PressedScreenPoint = screenPoint;
            }

            public void SetPoint(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
                InRange = true;
            }

            public void ReleaseAt(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
#if UNITY_EDITOR
                PressedScreenPoint = new(float.NaN, float.NaN);
#endif
            }

            public void SetOutOfRange()
            {
                InRange = false;
            }
        }

        private struct InputCoordData
        {
            public NoteCoord Coord { get; private set; }
            public NoteCoord PressedCoord { get; private set; }

            public void PressAt(NoteCoord coord)
            {
                Coord = coord;
                PressedCoord = coord;
            }

            public void MovedTo(NoteCoord coord)
            {
                Coord = coord;
            }
        }

    }
}