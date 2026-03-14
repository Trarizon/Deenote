#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class SelectionAreaRect : MonoBehaviour
    {
        [SerializeField] GameStageController _gameStage = default!;
        [SerializeField] RectTransform _dragSelectionArea = default!;

        public GameStageController GameStage => _gameStage;

        public void SetSelectionArea(NoteCoord startCoord,NoteCoord endCoord)
        {
            var currentTime = _gameStage.GamePlay.MusicPlayer.Time;

            var (xMin, zMin) = GameStage.EvaluateNoteWorldXZ(startCoord - new NoteCoord(0, currentTime));
            var (xMax, zMax) = GameStage.EvaluateNoteWorldXZ(endCoord - new NoteCoord(0, currentTime));

            _dragSelectionArea.gameObject.SetActive(true);
            _dragSelectionArea.offsetMin = new(xMin, zMin);
            _dragSelectionArea.offsetMax = new(xMax, zMax);
        }

        public void HideSelectionArea()
        {
            _dragSelectionArea.gameObject.SetActive(false);
        }
    }
}
