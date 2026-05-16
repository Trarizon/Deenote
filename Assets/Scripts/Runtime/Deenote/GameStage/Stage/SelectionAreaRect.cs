#nullable enable

using Deenote.CoreB.Models;
using Deenote.Editing.NoteSelection;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.Stage
{
    [MovedFrom("Deenote.Core.GameStage")]
    internal sealed class SelectionAreaRect : MonoBehaviour
    {
        [SerializeField] GameStageController _gameStage = default!;
        [SerializeField] RectTransform _dragSelectionArea = default!;

        private GameStageContext _context;

        public GameStageController GameStage => _gameStage;

        private StageDragSelector _dragSelector = default!;

        private void Awake()
        {
            _context = MainSystem.Contexts.GameStage;
        }

        internal void Initialize(StageDragSelector dragSelector)
        {
            _dragSelector = dragSelector;
            _dragSelector.SelectionAreaChanged += _dragSelector_SelectionAreaChanged;
        }

        private void _dragSelector_SelectionAreaChanged(StageDragSelector s, StageDragSelector.SelectionAreaChangedEventArgs e)
        {
            SetSelectionArea(e.Start, e.End);
        }

        private void OnDestroy()
        {
            _dragSelector.SelectionAreaChanged -= _dragSelector_SelectionAreaChanged;
        }

        public void SetSelectionArea(NoteCoord startCoord, NoteCoord endCoord)
        {
            var currentTime = _context.NotesContext.CurrentTime;

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
