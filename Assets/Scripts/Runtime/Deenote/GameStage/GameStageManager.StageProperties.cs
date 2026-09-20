using Deenote.Core.GameStage;
using Deenote.CoreB.Notification;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Replica;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageManager
    {
        public float MusicTime => MainSystem.GamePlayManager.MusicPlayer.Time;

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        [MemberNotNull(nameof(GameStage))]
        public void AssertStageLoaded() => Debug.Assert(GameStage is not null, "Stage not loaded");

        [MemberNotNullWhen(true, nameof(GameStage))]
        public bool IsStageLoaded() => GameStage is not null;

        #region Stage Timing

        public float StageNoteActiveAheadTime
        {
            get {
                return Config.NoteAppearAheadTimeFactor / Strategy.GetNoteFallSpeedInternal(ActualNoteFallSpeed);
            }
        }

        public float GetStageNoteActiveAheadTime(float noteSpeed) => StageNoteActiveAheadTime / GetDisplayNoteSpeed(noteSpeed);
        public float GetStageNoteActiveTime(IStageNoteNode node) => node.Time - GetStageNoteActiveAheadTime(node.Speed);

        public float StageNoteAppearAheadTime => StageNoteActiveAheadTime * VisibleRangePercentage;
        public float GetStageNoteAppearAheadTime(float noteSpeed) => StageNoteAppearAheadTime / GetDisplayNoteSpeed(noteSpeed);
        public float GetStageNoteAppearTime(IStageNoteNode node) => node.Time - GetStageNoteAppearAheadTime(node.Speed);

        public float GetDisplayNoteSpeed(float speed) => IsApplySpeedDifference ? speed : 1f;

        public float GetNotePseudoTime(float time, float noteSpeed)
        {
            var currentTime = MusicTime;
            return currentTime + (time - currentTime) * GetDisplayNoteSpeed(noteSpeed);
        }

        #endregion

        #region VisibleRangePercentage

        private float? _cacheVisibleRangePercentage;

        public float VisibleRangePercentage
        {
            get {
                AssertStageLoaded();
                if (_cacheVisibleRangePercentage is null) {
                    var x = ConvertNoteCoordPositionToWorldX(0f);
                    var maxZ = ConvertNoteCoordTimeToWorldZ(StageNoteActiveAheadTime);
                    var minZ = ConvertNoteCoordTimeToWorldZ(0f);

                    bool try0, try1;
                    try0 = GameStage.TryConvertNotePanelPositionToRaycastingViewportPoint((x, minZ), out var minVp);
                    try1 = GameStage.TryConvertNotePanelPositionToRaycastingViewportPoint((x, maxZ), out var maxVp);
                    Debug.Assert(try0 && try1);

                    var vp = new Vector2(maxVp.x, Mathf.Lerp(maxVp.y, minVp.y, SuddenPlus));
                    try0 = GameStage.TryConvertRaycastingViewportPointToNotePanelPosition(vp, out var pos);
                    Debug.Assert(try0);

                    _cacheVisibleRangePercentage = Mathf.InverseLerp(minZ, maxZ, pos.Z);
                }
                return _cacheVisibleRangePercentage.GetValueOrDefault();
            }
        }

        #endregion

        #region HighlightedNoteSpeed

        private const float ZeroAvoidHighlightedSpeed = 0.1f;

        private float _highlightedNoteSpeed_bf = 1f;

        public float HighlightedNoteSpeed
        {
            get => _highlightedNoteSpeed_bf;
            set {
                if (value <= 0f)
                    value = ZeroAvoidHighlightedSpeed;
                if (Utils.SetField(ref _highlightedNoteSpeed_bf, value)) {
                    PropertyChanged?.Invoke(this, nameof(HighlightedNoteSpeed));
                }
            }
        }

        public bool IsNoteHighlighted(NoteModel note)
        {
            return !IsFilterNoteSpeed || Mathf.Approximately(note.Speed, HighlightedNoteSpeed);
        }

        #endregion

        #region Converters

        public float ConvertWorldXToNoteCoordPosition(float x)
            => x / Config.NotePosToWorldXFactor;

        public float ConvertWorldZToNoteCoordTime(float z, float noteSpeed = 1f)
            => ConvertWorldZToNoteCoordTimeBase(z) / ActualNoteFallSpeed / GetDisplayNoteSpeed(noteSpeed);

        public float ConvertNoteCoordPositionToWorldX(float position)
            => position * Config.NotePosToWorldXFactor;

        public float ConvertNoteCoordTimeToWorldZ(float time, float noteSpeed = 1f)
            => ActualNoteFallSpeed * GetDisplayNoteSpeed(noteSpeed) * ConvertNoteCoordTimeToWorldZBase(time);

        public float ConvertNoteCoordTimeToHoldScaleY(float time, float noteSpeed = 1f)
            => ActualNoteFallSpeed * GetDisplayNoteSpeed(noteSpeed) * ConvertNoteCoordTimeToWorldZBase(time);

        public (float X, float Z) ConvertNoteCoordToWorldPosition(NoteCoord coord, float noteSpeed = 1f)
            => (ConvertNoteCoordPositionToWorldX(coord.Position), ConvertNoteCoordTimeToWorldZ(coord.Time, noteSpeed));

        public bool TryConvertPerspectiveViewportPointToNoteCoord(Vector2 perspectiveViewPanelViewportPoint, float noteSpeed, out NoteCoord coord)
        {
            AssertStageLoaded();

            if (!IsInViewArea(perspectiveViewPanelViewportPoint)) {
                coord = default;
                return false;
            }

            var stage = GameStage;
            var raycastViewportPoint = stage.ConvertPerspectiveViewportPointToRaycastingViewportPoint(perspectiveViewPanelViewportPoint);
            if (stage.TryConvertRaycastingViewportPointToNotePanelPosition(raycastViewportPoint, out var notePanelPosition)) {
                coord = new NoteCoord(
                    ConvertWorldXToNoteCoordPosition(notePanelPosition.X),
                    ConvertWorldZToNoteCoordTime(notePanelPosition.Z, noteSpeed) + MusicTime);
                return true;
            }

            coord = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        internal bool TryRaycastPerspectiveViewportPointToNote(Vector2 perspectiveViewPanelViewportPoint, [MaybeNullWhen(false)] out GameStageNoteController note)
        {
            AssertStageLoaded();

            if (!IsInViewArea(perspectiveViewPanelViewportPoint)) {
                note = default;
                return false;
            }
            var stage = GameStage;

            var raycastViewportPoint = stage.ConvertPerspectiveViewportPointToRaycastingViewportPoint(perspectiveViewPanelViewportPoint);
            if (stage.TryRaycastRaycastingViewportPointToNote(raycastViewportPoint, out note)) {
                return true;
            }
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        private float ConvertNoteCoordTimeToWorldZBase(float time)
            => time * Strategy.GetNoteFallSpeedInternal(ActualNoteFallSpeed) * Config.NoteTimeToWorldZFactor;

        private float ConvertWorldZToNoteCoordTimeBase(float z)
            => z / Strategy.GetNoteFallSpeedInternal(ActualNoteFallSpeed) / Config.NoteTimeToWorldZFactor;

        #endregion
    }
}