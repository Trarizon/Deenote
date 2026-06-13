#nullable enable

using Deenote.CoreB.Models;
using Deenote.CoreB.Notification;
using Deenote.Editing.NotePlacement;
using Deenote.GamePlay;
using Deenote.Library;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.Stage
{
    [MovedFrom("Deenote.Core.GameStage")]
    internal sealed class PlacementNotePlaneController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage;
        [SerializeField] Transform _content;

        private GamePlayContext _gamePlay = default!;
        private NotePlacementContext _placement = default!;
        private GameStageContext _stageContext = default!;

        public GameStageController GameStage => _stage;

        public Transform ContentTransform => _content;

        internal void OnInstantiate(GamePlayContext gamePlay, NotePlacementContext placement, GameStageContext stageContext)
        {
            _gamePlay = gamePlay;
            _placement = placement;
            _stageContext = stageContext;

            _placement.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.AnchorCoord))) {
                    var localCoord = s.AnchorCoord - new NoteCoord(0, _gamePlay.CurrentTime);
                    var (x, Z) = GameStage.ThemeEntry.NoteCoordStrategy.CoordToXZ(localCoord, _stageContext.ActualNoteFallSpeed, _stageContext.HighlightedNoteSpeed);
                    ContentTransform.WithLocalPositionXZ(x, Z);
                }
            });

            _stageContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.ActualIndicatorsVisible))) {
                    ContentTransform.gameObject.SetActive(s.ActualIndicatorsVisible);
                }
            });
        }
    }
}