#nullable enable

using Deenote.CoreB.Models;
using Deenote.Editing.EditorModels;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public abstract class PlacementNoteIndicatorController : MonoBehaviour
    {
        private const float NoteAlpha = 0.5f;

        private PlacementNotePlaneController _plane = default!;

        private Vector2 _localPosition;
        protected Vector2? _linkLineEndOffset;
        protected NotePrototypeModel _note = default!;

        public NotePrototypeModel NotePrototype => _note;

        public GameStageController GameStage => _plane.GameStage;

        internal void OnInstantiate(PlacementNotePlaneController plane)
        {
            _plane = plane;

            _note = new();

            var stage = _plane.GameStage;
            //game.AssertStageLoaded();
            stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal protected abstract void Refresh();

        internal void Initialize(NotePrototypeModel note)
        {
            _note = note;
            Refresh();
        }

        private void OnDestroy()
        {
            _plane.GameStage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _linkLineEndOffset = null;
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            var showLinkLine = GameStage.GamePlay.IsShowLinkLines;
            if (showLinkLine && _linkLineEndOffset is { } offset) {
                GameStage.GamePlay.AssertStageLoaded();

                var args = GameStage.GridLineArgs;
                collector.AddLine(_localPosition, _localPosition + offset,
                    args.LinkLineColor with { a = NoteAlpha },
                    args.LinkLineWidth);
            }
        }

        public void MoveTo(NoteCoord coord)
        {
            var localCoord = coord - new NoteCoord(position: 0, time: GameStage.GamePlay.MusicPlayer.Time);
            var (x, z) = GameStage.EvaluateNoteWorldXZ(localCoord, NotePrototype.Speed);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);
        }
    }
}