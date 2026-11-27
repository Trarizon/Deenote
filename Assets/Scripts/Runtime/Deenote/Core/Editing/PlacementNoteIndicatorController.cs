#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.CoreB.Unity;
using UnityEngine;
using Deenote.GameStage;
using Deenote.EditorModels;
using System;

namespace Deenote.Core.Editing
{
    public abstract class PlacementNoteIndicatorController : MonoBehaviour
    {
        private const float NoteAlpha = 0.5f;

        protected StageNotePlacer _placer = default!;
        protected GameStageController _stage = default!;

        private Vector2 _localPosition;
        protected Vector2? _linkLineEndOffset;
        protected NoteModel _note = default!;
        [Obsolete]
        public NoteModel NotePrototype => _note;

        private NotePrototypeModel _prototype = default!;
        protected NotePrototypeModel Prototype => _prototype;

        internal void OnInstantiate(StageNotePlacer placer)
        {
            // _placer = placer;

            // _note = new();
            _stage = placer._editor._game.Stage!;
            // var game = _placer._editor._game;
            // game.AssertStageLoaded();
            // game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal void OnInstantiate(GameStageController stage)
        {
            _stage = stage;
            _stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        private void RefreshData()
        {
            var localCoord = _prototype.PositionCoord - new NoteCoord(position: 0, time: App.GamePlayManager.CurrentTime);
            var (x, z) = App.GameStageManager.ConvertCoordToWorld(localCoord, Prototype.Speed, true);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);

            Refresh();
        }

        internal protected abstract void Refresh();

        internal void Initialize(NoteModel note)
        {
            _prototype = new();
            _prototype.FromDataNonLinkInfo(note, default);
            RefreshData();
        }

        internal void Initialize(NotePrototypeModel prototype)
        {
            _prototype = prototype;
            RefreshData();
        }

        private void OnDestroy()
        {
            // var game = _placer._editor._game;
            // if (game.IsStageLoaded())
            //     game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
            _stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _linkLineEndOffset = null;
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            var showLinkLine = App.GameStageManager.IsShowLinkLines;
            if (showLinkLine && _linkLineEndOffset is { } offset) {

                var configs = _stage.GridLineConfig;
                collector.AddLine(_localPosition, _localPosition + offset,
                    configs.LinkLineData.ColorWithAlpha(((DeemoPlacementNoteIndicatorController)this)._config.IndicatorAlpha),
                    configs.LinkLineData.Width);
            }
        }

        [Obsolete("Location info is got from prototype.PositionCoord")]
        public void MoveTo(NoteCoord coord)
        {
            return;

            var localCoord = coord - new NoteCoord(position: 0, time: App.GamePlayManager.CurrentTime);
            var (x, z) = App.GameStageManager.ConvertCoordToWorld(localCoord, Prototype.Speed, true);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);
        }
    }
}