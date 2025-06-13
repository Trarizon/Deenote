#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage.Notes
{
    internal abstract class GameStageSpeedChangeWarningNoteController : MonoBehaviour, IGameStageNoteController
    {
        /// <summary>
        /// The coord position where <see cref="GameStageSpeedChangeWarningNoteController"/>s appear
        /// </summary>
        internal const float SpritePosition = -3.25f;
        /// <summary>
        /// The left coord position of grid line
        /// </summary>
        internal const float LineStartPosition = -3f;
        /// <summary>
        /// The right coord position of grid line
        /// </summary>
        internal const float LineEndPosition = 2f;

        protected GamePlayManager _game = default!;

        public SpeedChangeWarningModel Model { get; private set; } = default!;

        IStageSelectableNode IGameStageNoteController.Model => Model;

        private DisplayState _state;

        internal void OnInstantiate(GamePlayManager game)
        {
            _game = game;
            _game.AssertStageLoaded();
            _game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
            transform.WithLocalPositionX(_game.ConvertNoteCoordPositionToWorldX(SpritePosition));
        }

        internal void Initialize(SpeedChangeWarningModel model)
        {
            Model = model;
            RefreshVisual();
        }

        private void OnDestroy()
        {
            if (_game.IsStageLoaded())
                _game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _state = DisplayState.Inactive;
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            _game.AssertStageLoaded();

            if (_state is DisplayState.Active) {
                var xl = _game.ConvertNoteCoordPositionToWorldX(LineStartPosition);
                var xr = _game.ConvertNoteCoordPositionToWorldX(LineEndPosition);
                var z = _game.ConvertNoteCoordTimeToWorldZ(Model.Time - _game.MusicPlayer.Time, _game.HighlightedNoteSpeed);
                collector.AddLine(new(xl, z), new(xr, z),
                    _game.Stage.GridLineArgs.SpeedChangeLineColor,
                    _game.Stage.GridLineArgs.SpeedChangeLineWidth);
            }
        }

        #region Refresh

        public void RefreshStageDeltaTime()
        {
            var stageDeltaTime = Model.Time - _game.MusicPlayer.Time;
            SetState(stageDeltaTime);
            switch (_state) {
                case DisplayState.Active:
                    SetPositionZ(stageDeltaTime);
                    break;
                case DisplayState.Inactive:
                default:
                    break;
            }
        }

        public void RefreshVisual()
        {
            RefreshColoring();
        }

        public void RefreshColoring()
        {
            if (_state is DisplayState.Active) {
                SetNoteSpriteColor();
            }
        }

        #endregion

        #region Setters

        private void SetPositionZ(float time)
        {
            _game.AssertStageLoaded();
            float z = _game.ConvertNoteCoordTimeToWorldZ(time, _game.HighlightedNoteSpeed);
            transform.WithLocalPositionZ(z);
        }

        private void SetNoteSpriteColor()
        {
            _game.AssertStageLoaded();
            var stage = _game.Stage;

            Color color;
            if (Model.IsSelected)
                color = stage.Args.NoteSelectedColor;
            else
                color = Color.white;
            SetNoteSpriteColorRGB(color);
        }

        protected abstract void SetNoteSpriteColorRGB(Color color);

        private void SetState(float stageDeltaTime)
        {
            DisplayState state;
            if (stageDeltaTime >= 0) {
                state = DisplayState.Active;
                RefreshColoring();
            }
            else
                state = DisplayState.Inactive;

            if (Utils.SetField(ref _state, state)) {
                OnStateChanged(_state);
            }
        }

        private void OnStateChanged(DisplayState state)
        {
            switch (state) {
                case DisplayState.Inactive:
                    gameObject.SetActive(false);
                    break;
                case DisplayState.Active:
                    gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        #endregion

        private enum DisplayState
        {
            Inactive,
            Active,
        }
    }
}