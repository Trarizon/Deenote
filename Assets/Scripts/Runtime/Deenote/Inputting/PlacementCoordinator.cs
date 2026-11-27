using Deenote.Core.Editing;
using Deenote.Editing;
using Deenote.Entities;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.GameStage.UI;
using UnityEngine;

namespace Deenote.Inputting
{
    public sealed partial class PlacementCoordinator
    {
        private readonly GamePlayManager2 _gamePlayManager;
        private readonly GameStageManager _stageManager;
        private readonly EditingManager _editingManager;

        private readonly StageDragSelector _dragSelector;
        private readonly StageNotesPlacer2 _placer;

        private readonly IGameStagePerspectiveViewPanel _perspectiveViewPanel;

        internal PlacementCoordinator(GamePlayManager2 gamePlayManager, GameStageManager stageManager, EditingManager editingManager, IGameStagePerspectiveViewPanel perspectiveViewPanel)
        {
            _gamePlayManager = gamePlayManager;
            _stageManager = stageManager;
            _editingManager = editingManager;
            _dragSelector = editingManager.DragSelector;
            _placer = editingManager.NotesPlacer;
            _perspectiveViewPanel = perspectiveViewPanel;
        }

        private bool _isPastingRequested;

        // Processors

        partial void EnterIdleEntry()
        {
            if (_isPastingRequested) {
                State = FsmState.IdlePaste;
                _isPastingRequested = false;
            }
            else if (_placer.PlaceSpeedWarningNote) {
                State = FsmState.IdleWarning;
            }
            else if (_placer.IsPlacingSlideRequested) {
                State = FsmState.IdleSlides;
            }
            else {
                State = FsmState.IdleSingle;
            }
        }

        // Single

        partial void EnterIdleSingle()
        {
            _placer.PrepareSingle();
        }

        partial void ProcessIdleSingle()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingIdle(coord);
        }

        partial void EnterPlacingSingle()
        {
            _placer.BeginPlaceSingle(_coordData.Coord);
        }

        partial void ProcessPlacingSingle()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceSingle(coord, _mouseData.DraggedDelta);
        }

        // Slides

        partial void EnterIdleSlides()
        {
            _placer.PrepareSlides();
        }

        partial void ProcessIdleSlides()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingIdle(coord);
        }

        partial void EnterPlacingSlides()
        {
            _placer.BeginPlaceSlides(_coordData.Coord);
        }

        partial void ProcessPlacingSlides()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceSlides(coord);
        }

        // Paste

        partial void EnterIdlePaste()
        {
            _placer.PrepareTemplate(_editingManager.NotesClipBoard.Notes, _editingManager.NotesClipBoard.BaseCoord);
        }

        partial void ProcessIdlePaste()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceTemplate(coord);
        }

        partial void ProcessPlacingPaste()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, true, out var coord)) {
                return;
            }
            _placer.MovingPlaceTemplate(coord);
        }

        // Place

        partial void EnterPlaced()
        {
            _placer.EndPlace();
            State = FsmState.IdleEntry;
        }

        // Select

        partial void EnterSelecting()
        {
            _dragSelector.BeginDragSelect(_coordData.PressedCoord);
        }

        partial void ProcessSelecting()
        {
            if (!TryConvertScreenPointToNoteCoord(_mouseData.ScreenPoint, false, out var coord)) {
                return;
            }
            _dragSelector.DraggingSelect(coord);
        }

        partial void ExitSelecting()
        {
            _dragSelector.EndDragSelect(_mouseData.ScreenPoint);
        }

        partial void EnterSelected()
        {
            State = FsmState.IdleEntry;
        }

        #region Helpers

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, bool applyPlacementNoteSpeed, out NoteCoord coord)
        {
            if (!_perspectiveViewPanel.TryConvertScreenPointToViewportPoint(screenPoint, out var vp)) {
                coord = default;
                return false;
            }

            var res = _stageManager.TryConvertPerspectiveViewportPointToCoord(vp,
                applyPlacementNoteSpeed ? MainSystem.StageChartEditor.Placer.PlacingNoteSpeed : 1f, out coord);

            coord.Time += _gamePlayManager.CurrentTime;
            return res;
        }

        private bool IsInNoteSelectionArea(NoteCoord coord)
        {
            return true;
        }

        private bool IsInNotePlacementArea(NoteCoord coord)
        {
            return true;
        }

        #endregion
    }
}