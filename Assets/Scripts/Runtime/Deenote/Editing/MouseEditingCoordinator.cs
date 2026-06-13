#nullable enable

using Deenote.CoreB.Models;
using Deenote.Editing.NotePlacement;
using Deenote.Editing.NoteSelection;
using Deenote.GamePlay;
using Deenote.GameStage;
using UnityEngine;

namespace Deenote.Editing
{
    public sealed partial class MouseEditingCoordinator
    {
        #region Constants

        private const float SelectionAreaMaxPosition = 6f;
        private const float SelectionAreaMinPosition = -6f;

        #endregion

        private readonly StageNotePlacer2 _placer;
        private readonly StageDragSelector _selector;
        private readonly GameStageContext _stage;
        private readonly GamePlayContext _gamePlay;
        private readonly EditorContext _editor;

        internal MouseEditingCoordinator(StageNotePlacer2 placer, StageDragSelector selector, GameStageContext stage, GamePlayContext gamePlay, EditorContext editor)
        {
            _placer = placer;
            _selector = selector;
            _stage = stage;
            _gamePlay = gamePlay;
            _editor = editor;

            RegisterEvents();

            State = FsmState.EntryIdle;
        }

        private bool IsInNoteSelectionArea(NoteCoord coord)
        {
            return true;
        }

        private void EnterSuperPlacement()
        {
            _placer.IndicatorsForceVisible = null;
        }

        private void ExitSuperPlacement()
        {
            _placer.IndicatorsForceVisible = false;
        }

        private void EnterSuperSelection()
        {
            _selector.Enabled = true;
        }

        private void ExitSuperSelection()
        {
            _selector.Enabled = false;
        }

        #region Idle

        private void EnterEntryIdle()
        {
            if (!_mouseInuputData.InRange) {
                State = FsmState.IdleOutOfRange;
            }
            else if (_placer.IsPastingRequested) {
                State = FsmState.IdlePaste;
            }
            else if (_placer.IsPlacingSlidesRequested) {
                State = FsmState.IdleSlides;
            }
            else {
                State = FsmState.IdleSingle;
            }
        }

        private void EnterIdleSingle()
        {
            _placer.PrepareSingle();
        }

        private void ProcessIdleSingle()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingIdle(coord);
        }

        private void EnterIdleSlides()
        {
            _placer.PrepareSlide();
        }

        private void ProcessIdleSlides()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingIdle(coord);
        }

        private void EnterIdlePaste()
        {
            _placer.PrepareTemplateNotes(_editor.ClipBoard.Notes);
        }

        private void ProcessIdlePaste()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceTemplate(coord);
        }

        #endregion

        #region Placing

        private void EnterPlacingSingle()
        {
            _placer.BeginPlaceSingleNote(_inputCoordData.Coord);
        }

        private void ProcessPlacingSingle()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceSingle(coord, _mouseInuputData.DraggedDelta, 0);
        }

        private void EnterPlacingSlides()
        {
            _placer.BeginPlaceSlides(_inputCoordData.Coord);
        }

        private void ProcessPlacingSlides()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceSlides(coord);
        }

        private void EnterPlaced()
        {
            _placer.EndPlace();
            State = FsmState.EntryIdle;
        }

        #endregion

        #region Selection

        private void EnterSelecting()
        {
            _selector.BeginDragSelect(_inputCoordData.Coord);
        }

        private void ProcessSelecting()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseInuputData.ScreenPoint, false, out var coord)) {
                return;
            }
            _selector.DraggingSelect(coord);
        }

        private void EnterSelected()
        {
            if (TryConvertScreenPointToViewportPoint(_mouseInuputData.ScreenPoint, out var viewportPoint)) {
                _selector.EndDragSelect(viewportPoint);
            }
            else {
                _selector.EndDragSelect(null);
            }
            State = FsmState.EntryIdle;
        }

        #endregion

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, bool applyPlacementNoteSpeed, out NoteCoord coord)
        {
            coord = default;
            if (_stage.GameStage is null) {
                return false;
            }

            if (!_stage.PerspectiveViewPanelInfo.TryConvertScreenPointToViewportPoint(screenPoint, out var viewportPoint)) {
                return false;
            }

            if (!_stage.GameStage.TryEvaluatePerspectiveViewportPointToLocalNoteCoord(viewportPoint, applyPlacementNoteSpeed ? _editor.NotePlacement.PlacementNoteSpeed : 1, out coord)) {
                return false;
            }

            coord.Time += _gamePlay.CurrentTime;

            return true;
        }

        private bool TryConvertScreenPointToViewportPoint(Vector2 screenPoint, out Vector2 viewportPoint)
        {
            viewportPoint = default;
            if (_stage.GameStage is null) {
                return false;
            }

            if (!_stage.PerspectiveViewPanelInfo.TryConvertScreenPointToViewportPoint(screenPoint, out viewportPoint)) {
                return false;
            }

            return true;
        }
    }
}
