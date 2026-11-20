using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GameStage.World;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageManager
    {
        public float ApplySpeedDiff(float speed)
            => IsApplySpeedDifference ? speed : 1f;

        private float ApplySpeedDiff(float speed, bool applySpeedDiffSetting)
            => !applySpeedDiffSetting || IsApplySpeedDifference ? speed : 1f;

        public float GetNotePseudoTime(IStageNoteNode node, float currentTime)
            => currentTime + (node.Time - currentTime) * ApplySpeedDiff(node.Speed);

        public float GetStageNoteActiveAheadTime(float noteSpeed)
            => StandardNoteActiveAheadTime / noteSpeed;

        public float GetStageNoteActiveAheadTime(float noteSpeed, bool applySpeedDiffSetting)
            => GetStageNoteActiveAheadTime(ApplySpeedDiff(noteSpeed, applySpeedDiffSetting));

        public float GetStageNoteActiveTime(IStageNoteNode node, bool applySpeedDiffSetting = false)
            => node.Time - GetStageNoteActiveAheadTime(node.Speed, applySpeedDiffSetting);

        public float StandardNoteAppearAheadTime => StandardNoteActiveAheadTime * GameStage!.VisibleRangePercentage;

        public float GetStageNoteAppearAheadTime(float noteSpeed)
            => StandardNoteAppearAheadTime / noteSpeed;

        public float GetStageNoteAppearAheadTime(float noteSpeed, bool applySpeedDiffSetting = false)
            => GetStageNoteAppearAheadTime(ApplySpeedDiff(noteSpeed, applySpeedDiffSetting));

        public float GetStageNoteAppearTime(IStageNoteNode node, bool applySpeedDiffSetting = false)
            => node.Time - GetStageNoteAppearAheadTime(node.Speed, applySpeedDiffSetting);

        // All coords are local coords, call site should manually add GamePlayManager.CurrentTime if needed.

        public float ConvertWorldXToCoordPosition(float x)
            => x / Config.NotePosToWorldXFactor;

        public float ConvertCoordPositionToWorldX(float position)
            => position * Config.NotePosToWorldXFactor;

        public float ConvertWorldZToCoordTime(float z, float noteSpeed = 1f, bool applySpeedDiffSetting = false)
            => z / (Config.NoteTimeToWorldZFactor * ApplySpeedDiff(noteSpeed, applySpeedDiffSetting) * Strategy.GetNoteFallSpeedInternal(ActualNoteFallSpeed));

        public float ConvertCoordTimeToWorldZ(float time, float noteSpeed = 1f, bool applySpeedDiffSetting = false)
            => time * Config.NoteTimeToWorldZFactor * ApplySpeedDiff(noteSpeed, applySpeedDiffSetting) * Strategy.GetNoteFallSpeedInternal(ActualNoteFallSpeed);

        public float ConvertCoordTimeToWorldScaleY(float time, float noteSpeed = 1f, bool applySpeedDiffSetting = false)
            // Stage!.Args.HoldSpritePrefab.Sprite.bounds.size.y;
            => time * Config.NoteTimeToWorldZFactor * ApplySpeedDiff(noteSpeed, applySpeedDiffSetting) * Strategy.GetNoteFallSpeedInternal(ActualNoteFallSpeed);

        public (float X, float Z) ConvertCoordToWorld(NoteCoord coord, float noteSpeed = 1f, bool applySpeedDiffSetting = false)
            => (ConvertCoordPositionToWorldX(coord.Position), ConvertCoordTimeToWorldZ(coord.Time, noteSpeed, applySpeedDiffSetting));

        public NoteCoord ConvertWorldToCoord(float x, float z, float noteSpeed = 1f, bool applySpeedDiffSetting = false)
            => new(ConvertWorldXToCoordPosition(x), ConvertWorldZToCoordTime(z, noteSpeed, applySpeedDiffSetting));

        public bool TryConvertPerspectiveViewportPointToCoord(Vector2 viewportPoint, float noteSpeed, out NoteCoord coord)
        {
            if (!IsInViewArea(viewportPoint)) {
                coord = default;
                return false;
            }

            if (ThemeManager.CurrentStage is not { } stage) {
                App.Logger.LogDebug("CurrentStage is null while try converting viewport point to coord");
                coord = default;
                return false;
            }

            var raycastingViewportPoint = stage.ConvertPerspectiveViewportPointToRaycastingViewportPoint(viewportPoint);
            var ray = stage.GetRaycastingViewportRay(raycastingViewportPoint);
            if (!stage.NotePlane.TryRaycast(ray, out var hit)) {
                coord = default;
                return false;
            }

            coord = ConvertWorldToCoord(hit.x, hit.z, noteSpeed);
            return true;

        }

        internal bool TryRaycastPerspectiveViewportPointToNote(Vector2 viewportPoint, [MaybeNullWhen(false)] out GameStageNoteController note)
            => TryRaycastPerspectiveViewportPointToNote<GameStageNoteController>(viewportPoint, out note);

        internal bool TryRaycastPerspectiveViewportPointToWarningNote(Vector2 viewportPoint, [MaybeNullWhen(false)] out GameStageSpeedChangeWarningNoteController note)
            => TryRaycastPerspectiveViewportPointToNote<GameStageSpeedChangeWarningNoteController>(viewportPoint, out note);

        private bool TryRaycastPerspectiveViewportPointToNote<T>(Vector2 viewportPoint, [MaybeNullWhen(false)] out T note)
        {
            if (!IsInViewArea(viewportPoint)) {
                note = default;
                return false;
            }

            if (ThemeManager.CurrentStage is not { } stage) {
                App.Logger.LogDebug("CurrentStage is null while try raycasting viewport point to collider");
                note = default;
                return false;
            }

            var raycastingViewportPoint = stage.ConvertPerspectiveViewportPointToRaycastingViewportPoint(viewportPoint);
            var ray = stage.GetRaycastingViewportRay(raycastingViewportPoint);
            if (!Physics.Raycast(ray, out var hitInfo)) {
                var collider = hitInfo.collider;
                if (collider != null) {
                    if (collider.TryGetComponent<GameStageNoteRaycastingCollider<T>>(out var noteCollider)) {
                        note = noteCollider.Note;
                        return true;
                    }
                }
            }

            note = default;
            return false;
        }

        private static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
    }
}
