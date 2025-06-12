#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class GameStageSpeedChangeWarningNoteController : MonoBehaviour
    {
        /// <summary>
        /// The coord position where <see cref="GameStage.GameStageSpeedChangeWarningNoteController"/>s appear
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

        private GamePlayManager _game = default!;

        public SpeedChangeWarningModel Model { get; private set; } = default!;

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

        #endregion

        #region Setters

        private void SetPositionZ(float time)
        {
            _game.AssertStageLoaded();
            float z = _game.ConvertNoteCoordTimeToWorldZ(time, _game.HighlightedNoteSpeed);
            transform.WithLocalPositionZ(z);
        }

        private void SetState(float stageDeltaTime)
        {
            DisplayState state;
            if (stageDeltaTime >= 0)
                state = DisplayState.Active;
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