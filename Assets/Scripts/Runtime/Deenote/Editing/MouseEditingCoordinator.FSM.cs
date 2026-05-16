#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Notification;
using UnityEngine;

namespace Deenote.Editing
{
    partial class MouseEditingCoordinator
    {
        private MouseInuputData _mouseInuputData;
        private InputCoordData _inputCoordData;

        private FsmState _state_bf;
        private FsmState State
        {
            get => _state_bf;
            set {
                if (_state_bf == value) return;

                Debug.Log($"[{nameof(MouseEditingCoordinator)}] Exit state {_state_bf}");

                var oldSuper = SuperStateOf(_state_bf);
                var newSuper = SuperStateOf(value);
                if (oldSuper != newSuper) {
                    Debug.Log($"[{nameof(MouseEditingCoordinator)}] Exit super state {oldSuper}");
                    switch (oldSuper) {
                        case FsmSuperState.Placing: ExitSuperPlacement(); break;
                        case FsmSuperState.Selecting: ExitSuperSelection(); break;
                    }
                }

                _state_bf = value;

                if (oldSuper != newSuper) {
                    Debug.Log($"[{nameof(MouseEditingCoordinator)}] Enter super state {newSuper}");
                    switch (newSuper) {
                        case FsmSuperState.Placing: EnterSuperPlacement(); break;
                        case FsmSuperState.Selecting: EnterSuperSelection(); break;
                    }
                }

                Debug.Log($"[{nameof(MouseEditingCoordinator)}] Enter state {value}");
                switch (value) {
                    case FsmState.EntryIdle: EnterEntryIdle(); break;
                    case FsmState.IdleSingle: EnterIdleSingle(); break;
                    case FsmState.IdleSlides: EnterIdleSlides(); break;
                    case FsmState.IdlePaste: EnterIdlePaste(); break;
                    case FsmState.PlacingSingle: EnterPlacingSingle(); break;
                    case FsmState.PlacingSlides: EnterPlacingSlides(); break;
                    case FsmState.Placed: EnterPlaced(); break;
                    case FsmState.Selecting: EnterSelecting(); break;
                    case FsmState.Selected: EnterSelected(); break;
                }

                static FsmSuperState SuperStateOf(FsmState state)
                    => (FsmSuperState)(state & SuperStateMask);
            }
        }

        private void RegisterEvents()
        {
            _gamePlay.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentTime))) {
                    Process();
                }
            });

            _placer.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.IsPastingRequested))) {
                    if (State is FsmState.EntryIdle or FsmState.IdleSingle or FsmState.IdleSlides) {
                        if (s.IsPastingRequested) {
                            State = FsmState.IdlePaste;
                        }
                    }
                }
                if (e.MatchProperty(nameof(s.IsPlacingSlidesRequested))) {
                    if (s.IsPlacingSlidesRequested) {
                        if (State is FsmState.EntryIdle or FsmState.IdleSingle) {
                            State = FsmState.IdleSlides;
                        }
                    }
                    else {
                        if (State is FsmState.EntryIdle or FsmState.PlacingSlides) {
                            State = FsmState.IdleSingle;
                        }
                    }
                }
            });
        }

        public void LeftMouseDown(Vector2 screenPoint)
        {
            _mouseInuputData.SetPointAsPress(screenPoint);
            _mouseInuputData.LeftMouseDown = true;

            switch (State) {
                case FsmState.IdleSingle:
                case FsmState.IdleSlides:
                case FsmState.IdlePaste:
                    if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, false, out var coord))
                        return;
                    if (!IsInNoteSelectionArea(coord))
                        return;
                    _inputCoordData.SetCoordAsPress(coord);
                    State = FsmState.Selecting;
                    break;
                case FsmState.PlacingSingle:
                case FsmState.PlacingSlides:
                    State = FsmState.Aborted;
                    break;
            }
        }

        public void LeftMouseUp(Vector2 screenPoint)
        {
            _mouseInuputData.SetPointAsRelease(screenPoint);
            _mouseInuputData.LeftMouseDown = false;

            switch (State) {
                case FsmState.Selecting:
                    State = FsmState.Selected;
                    break;
                case FsmState.Aborted:
                    if (_mouseInuputData.LeftMouseDown || _mouseInuputData.RightMouseDown)
                        return;
                    State = FsmState.EntryIdle;
                    break;
            }
        }

        public void RightMouseDown(Vector2 screenPoint)
        {
            _mouseInuputData.SetPointAsPress(screenPoint);
            _mouseInuputData.RightMouseDown = true;
            switch (State) {
                case FsmState.IdleSingle:
                    if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out var coord))
                        return;
                    _inputCoordData.SetCoordAsPress(coord);
                    State = FsmState.PlacingSingle;
                    break;
                case FsmState.IdleSlides:
                    if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out coord))
                        return;
                    _inputCoordData.SetCoordAsPress(coord);
                    State = FsmState.PlacingSlides;
                    break;
            }
        }

        public void RightMouseUp(Vector2 screenPoint)
        {
            _mouseInuputData.SetPointAsRelease(screenPoint);
            _mouseInuputData.RightMouseDown = false;
            switch (State) {
                case FsmState.Aborted:
                    if (_mouseInuputData.LeftMouseDown || _mouseInuputData.RightMouseDown)
                        return;
                    State = FsmState.EntryIdle;
                    break;
                case FsmState.PlacingSingle:
                case FsmState.PlacingSlides:
                case FsmState.IdlePaste:
                    State = FsmState.Placed;
                    break;
            }
        }

        public void MouseMove(Vector2 screenPoint)
        {
            _mouseInuputData.SetPoint(screenPoint);
            Process();
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
            }
        }

        private struct MouseInuputData
        {
            public bool LeftMouseDown;
            public bool RightMouseDown;
            public Vector2 ScreenPoint { get; private set; }
            public Vector2 PressedScreenPoint { get; private set; }
            public readonly Vector2 DraggedDelta => ScreenPoint - PressedScreenPoint;

            public void SetPointAsPress(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
                PressedScreenPoint = screenPoint;
            }

            public void SetPoint(Vector2 screenPoint) => ScreenPoint = screenPoint;

            public void SetPointAsRelease(Vector2 screenPoint)
            {
                ScreenPoint = screenPoint;
#if UNITY_EDITOR
                PressedScreenPoint = new(float.NaN, float.NaN);
#endif
            }
        }

        private struct InputCoordData
        {
            public NoteCoord Coord { get; private set; }
            public NoteCoord PressedCoord { get; private set; }

            public void SetCoordAsPress(NoteCoord coord)
            {
                Coord = coord;
                PressedCoord = coord;
            }
        }

        private enum StateFlag
        {
            /// <summary>
            /// When the indicator is forced to be hidden, and all placement will be disabled
            /// </summary>
            Disabled,
            /// <summary>
            /// When both mouse key pressed
            /// </summary>
            Cancelling,
            Idle,
            Placing,
            Selecting,
        }

        private enum FsmState
        {
            EntryIdle = 0x10,
            IdleSingle,
            IdleSlides,
            IdlePaste,
            PlacingSingle,
            PlacingSlides,
            Placed,

            Selecting = 0x20,
            Selected,

            Aborted = 0x30,
        }

        private enum FsmSuperState
        {
            Placing = 0x10,
            Selecting = 0x20,
            Disabled = 0x30,
        }

        private const FsmState SuperStateMask = (FsmState)0xF0;
    }
}
