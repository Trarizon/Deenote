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

        private readonly StageDragSelector _dragSelector;

        private readonly IGameStagePerspectiveViewPanel _perspectiveViewPanel;

        internal PlacementCoordinator(GamePlayManager2 gamePlayManager, GameStageManager stageManager,
             StageDragSelector dragSelector, IGameStagePerspectiveViewPanel perspectiveViewPanel)
        {
            _gamePlayManager = gamePlayManager;
            _stageManager = stageManager;
            _dragSelector = dragSelector;
            _perspectiveViewPanel = perspectiveViewPanel;
        }

        // Processors

        partial void EnterSelecting()
        {
            _dragSelector.BeginDragSelect(_coordData.PressedCoord);
        }

        partial void ProcessSelecting()
        {
            _dragSelector.DraggingSelect(_coordData.Coord);
        }

        partial void ExitSelecting()
        {
            _dragSelector.EndDragSelect(_mouseData.ScreenPoint);
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