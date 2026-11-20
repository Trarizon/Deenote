#nullable enable

using Deenote.Core.GamePlay;
using Deenote.CoreB.Unity;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.Library;
using Deenote.Library.Mathematics;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    /// <summary>
    /// Lightweight stage view for a speed change warning. It only displays a selectable
    /// note body and a time-grid-like line; it has no hold or hit effects.
    /// </summary>
    internal abstract class GameStageSpeedChangeWarningNoteController : MonoBehaviour
    {
        /// <summary>
        /// Position of the warning body, in note-coordinate space.
        /// </summary>
        internal const float SpritePosition = -3f;

        [SerializeField] private SpeedChangeWarningLineRenderer _lineRenderer = default!;
        [SerializeField] private Collider _selectionCollider = default!;

        protected GameStageController _stage = default!;
        private GamePlayManager2 _gamePlayManager = default!;

        private float _stageDeltaTime;
        private bool _isActive;

        public SpeedChangeWarningModel Model { get; private set; } = default!;

        /// <summary>
        /// Time before the warning is hit at which the warning starts to appear.
        /// Warnings always move in the lane of <see cref="GameStageManager.HighlightedNoteSpeed"/>.
        /// </summary>
        private float AppearAheadTime
        {
            get {
                return _stage.StageManager.GetStageNoteAppearAheadTime(_stage.StageManager.HighlightedNoteSpeed ?? _stage.StageManager.PlacementNoteSpeed, true);
            }
        }

        internal void OnInstantiate(GameStageController stage)
        {
            _stage = stage;
            _lineRenderer.OnInstantiate(_stage);
            _gamePlayManager = App.GamePlayManager;
        }

        internal void Initialize(SpeedChangeWarningModel model)
        {
            Model = model;
            RefreshVisual();
            RefreshStageDeltaTime();
        }

        private void OnDisable()
        {
            _isActive = false;
            _lineRenderer?.SetVisible(false);
        }

        internal void RefreshStageDeltaTime()
        {
            if (Model is null)
                return;

            _stageDeltaTime = Model.Time - _gamePlayManager.CurrentTime;
            bool active = _stageDeltaTime >= 0f;
            if (active != _isActive) {
                _isActive = active;
                OnStateChanged(active);
            }

            if (active) {
                var speed = _stage.StageManager.HighlightedNoteSpeed ?? _stage.StageManager.PlacementNoteSpeed;
                transform.WithLocalPositionZ(
                    _stage.StageManager.ConvertCoordTimeToWorldZ(_stageDeltaTime, speed, true));

                bool timeGridVisible = _stage.StageManager.WarningNoteVisibility.HasFlag(GameStageWarningNoteVisibility.ShowLine);
                if (_selectionCollider is not null)
                    _selectionCollider.enabled = timeGridVisible;
                _lineRenderer?.SetTime(_stageDeltaTime, speed, timeGridVisible ? 1f : 0f);
                SetNoteSpriteAlpha(_stage.StageManager.WarningNoteVisibility.HasFlag(GameStageWarningNoteVisibility.ShowNote));
                RefreshColoring();
            }
        }

        internal void RefreshVisual()
        {
            if (Model is null)
                return;

            transform.WithLocalPositionX(_stage.StageManager.ConvertCoordPositionToWorldX(SpritePosition));
            SetNoteSprite();
            SetNoteSize();
            RefreshColoring();
            RefreshVisibility();
        }

        internal void RefreshVisibility()
        {
            _lineRenderer.SetVisible(_stage.StageManager.WarningNoteVisibility.HasFlag(GameStageWarningNoteVisibility.ShowNoteAndLine));
            SetNoteSpriteAlpha(_stage.StageManager.WarningNoteVisibility.HasFlag(GameStageWarningNoteVisibility.ShowNote));
        }

        internal void RefreshColoring()
        {
            if (!_isActive)
                return;

            SetNoteSpriteEditorStatus(selected: Model.IsSelected);
        }

        private void OnStateChanged(bool active)
        {
            if (_selectionCollider is not null) _selectionCollider.enabled = active;
            _lineRenderer?.SetVisible(active);
            SetBodyActive(active);
            if (active)
                RefreshColoring();
        }

        private void SetNoteSpriteAlpha(bool visible)
        {
            // Debug.Assert(_isActive);

            if (!visible) {
                SetNoteSpriteRendererAlpha(0f);
                return;
            }

            var appearAheadTime = AppearAheadTime;
            var noteFadeInEndTime = appearAheadTime * (1 - _stage.Config.NoteFadeInRatio);

            var alpha = MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0f, 1f);
            SetNoteSpriteRendererAlpha(alpha);
        }

        protected abstract void SetNoteSprite();

        protected abstract void SetNoteSize();

        protected abstract void SetNoteSpriteEditorStatus(bool selected);

        protected abstract void SetNoteSpriteRendererAlpha(float alpha);

        protected abstract void SetBodyActive(bool active);
    }
}
