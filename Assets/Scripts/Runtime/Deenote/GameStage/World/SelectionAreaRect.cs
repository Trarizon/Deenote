using Deenote.Editing;
using Deenote.Entities;
using Deenote.GamePlay;
using UnityEngine;

namespace Deenote.GameStage.World
{
    public sealed class SelectionAreaRect : MonoBehaviour
    {
        [SerializeField] private RectTransform _areaRect;

        private GamePlayManager2 _gamePlayManager = default!;
        private GameStageManager _stageManager = default!;
        private StageDragSelector _dragSelector = default!;

        internal void OnInstantiate(GamePlayManager2 gamePlayManager, GameStageManager stageManager, StageDragSelector dragSelector)
        {
            _gamePlayManager = gamePlayManager;
            _stageManager = stageManager;
            _dragSelector = dragSelector;
            _dragSelector.SelectionAreaChanged += _SelectionAreaChanged;
        }

        void OnDestroy()
        {
            _dragSelector.SelectionAreaChanged -= _SelectionAreaChanged;
        }

        private void _SelectionAreaChanged(StageDragSelector selector, StageDragSelector.SelectionAreaChangedEventArgs args)
        {
            App.Logger.DebugLogTrace($"SelectionAreaRect: {args.StartCoord} -> {args.EndCoord}");
            SetSelectionArea(args.StartCoord, args.EndCoord);
        }

        public void SetSelectionArea(NoteCoord startCoord, NoteCoord endCoord)
        {
            if (startCoord == endCoord) {
                _areaRect.gameObject.SetActive(false);
                return;
            }

            var currentTime = _gamePlayManager.CurrentTime;

            var (xMin, zMin) = _stageManager.ConvertCoordToWorld(startCoord - new NoteCoord(0, currentTime));
            var (xMax, zMax) = _stageManager.ConvertCoordToWorld(endCoord - new NoteCoord(0, currentTime));

            _areaRect.gameObject.SetActive(true);
            _areaRect.offsetMin = new Vector2(xMin, zMin);
            _areaRect.offsetMax = new Vector2(xMax, zMax);
        }
    }
}