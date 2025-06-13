#nullable enable

using Deenote.Core.GameStage;
using Deenote.Core.GameStage.Notes;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.Editing.Indicators
{
    internal sealed class SpeedChangeWarningIndicatorController : MonoBehaviour
    {
        private const float NoteAlpha = 0.5f;

        private StageNotePlacer _placer = default!;

        private float _localTime;

        internal void OnInstantiate(StageNotePlacer placer)
        {
            _placer = placer;

            var game = _placer.GamePlayManager;
            game.AssertStageLoaded();
            game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
            transform.WithLocalPositionX(game.ConvertNoteCoordPositionToWorldX(GameStageSpeedChangeWarningNoteController.SpritePosition));
        }

        private void OnDestroy()
        {
            var game = _placer.GamePlayManager;
            if (game.IsStageLoaded())
                game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (_placer.IndicatorVisibility is StageNotePlacer.PlacementArea.SpeedChangeWarning) {
                var game = _placer.GamePlayManager;
                game.AssertStageLoaded();
                var args = game.Stage.GridLineArgs;

                var xl = game.ConvertNoteCoordPositionToWorldX(GameStageSpeedChangeWarningNoteController.LineStartPosition);
                var xr = game.ConvertNoteCoordPositionToWorldX(GameStageSpeedChangeWarningNoteController.LineEndPosition);
                var z = game.ConvertNoteCoordTimeToWorldZ(_localTime, game.HighlightedNoteSpeed);
                collector.AddLine(new(xl, z), new(xr, z),
                    args.SpeedChangeLineColor with { a = args.SpeedChangeLineColor.a * NoteAlpha },
                    args.SpeedChangeLineWidth);
            }
        }

        public void MoveTo(float time)
        {
            var game = _placer.GamePlayManager;
            game.AssertStageLoaded();

            var localTime = time - game.MusicPlayer.Time;
            _localTime = localTime;
            var z = game.ConvertNoteCoordTimeToWorldZ(localTime, game.HighlightedNoteSpeed);
            transform.WithLocalPositionZ(z);
        }

        public SpeedChangeWarningModel CreateModel() => new(_localTime + _placer.GamePlayManager.MusicPlayer.Time);
    }
}