#nullable enable

using Deenote.Contexts;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage.Args;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Notification;
using Deenote.GameStage;
using Deenote.GameStage.Themes;
using Deenote.Library;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Contexts;
using TMPro;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public abstract class GameStageController : MonoBehaviour
    {
        [SerializeField] GameStageNotePlaneController _notePlane = default!;
        [SerializeField] PlacementNotePlaneController _indicatorPlane = default!;
        [SerializeField] SelectionAreaRect _selectionAreaRect = default!;
        [SerializeField] Camera _perspectiveCamera = default!;
        [SerializeField] PerspectiveLinesRenderer _perspectiveLineRenderer = default!;

        [SerializeField] GameStageConfig _config = default!;

        [Obsolete]
        public GamePlayManager GamePlay { get => MainSystem.GamePlayManager;  }
        internal GameStageNotePlaneController NotePlane => _notePlane;
        internal PlacementNotePlaneController IndicatorPlane => _indicatorPlane;
        internal SelectionAreaRect SelectionAreaRect => _selectionAreaRect;
        public Camera PerspectiveCamera => _perspectiveCamera;
        public PerspectiveLinesRenderer PerspectiveLinesRenderer => _perspectiveLineRenderer;

        internal GameStageConfig Config => _config;



        [field: SerializeField]
        public GameStageArgs Args { get; private set; } = default!;
        [field: SerializeField]
        public GridLineArgs GridLineArgs { get; private set; } = default!;

        private float _visibleRangeCullingRatio_bf;

        /// <summary>
        /// The actual sudden+ on plane
        /// </summary>
        public float VisibleRangeCullingRatio
        {
            get => _visibleRangeCullingRatio_bf;
            set {
                if (Utils.SetField(ref _visibleRangeCullingRatio_bf, value)) {
                    //OnVisiblaRangeCullingRatioChanged(value);
                }
            }
        }

        /// <summary>
        /// The actual speed used for calculating
        /// </summary>
        [Obsolete]
        private float NoteFallSpeedInternal { get; set; }

        /// <summary>
        /// The time from a note(speed=1) is activated to falls on the judge line
        /// </summary>
        /// <remarks>
        /// We start to track when note appears as if sudden+ is 0,
        /// and set its visibility according to <see cref="NoteAppearAheadTime"/>
        /// </remarks>
        public float NoteActiveAheadTime => _context.ThemeContext.CurrentTheme.NoteCoordStrategy.GetNoteActiveAheadTime(_context.ActualNoteFallSpeed, 1); //Config.NoteAppearAheadTimeFactor / NoteFallSpeedInternal;

        /// <summary>
        /// The time from a note(speed=1) appears to falls on the judge line
        /// </summary>
        public float NoteAppearAheadTime => NoteActiveAheadTime * VisibleRangeCullingRatio;

        private void Awake()
        {
            PerspectiveLinesRenderer.OnInstantiate(this);
            _context = MainSystem.Contexts.GameStage;
        }

        public GameStageThemeEntry ThemeEntry { get; private set; } = default!;

        internal void OnInstantiate(GameStageThemeEntry themeEntry)
        {
            ThemeEntry = themeEntry;
        }

        private GameStageContext _context;

        [Obsolete]
        protected internal virtual void Initialize(GamePlayManager gamePlayManager, ProjectContext projectContext)
        {
            ////GamePlay = gamePlayManager;
            ////GamePlay.RegisterNotification(
            ////    GamePlayManager.NotificationFlag.StageEffectOn,
            ////    _manager => IsStageEffectOn = _manager.IsStageEffectOn);
            //GamePlay.RegisterNotification(
            //    GamePlayManager.NotificationFlag.NoteSpeed,
            //    manager => NoteFallSpeedInternal = ConvertFallSpeedToPlaneSpeed(manager.ActualNoteFallSpeed));
            //GamePlay.RegisterNotification(
            //    GamePlayManager.NotificationFlag.SuddenPlus,
            //    manager => VisibleRangeCullingRatio = ConvertSuddenPlusToVisibleRangeCullingRatio(manager.SuddenPlus));
            ////IsStageEffectOn = gamePlayManager.IsStageEffectOn;
            //NoteFallSpeedInternal = ConvertFallSpeedToPlaneSpeed(gamePlayManager.ActualNoteFallSpeed);
            //VisibleRangeCullingRatio = ConvertSuddenPlusToVisibleRangeCullingRatio(gamePlayManager.SuddenPlus);

            ////SelectionAreaRect.Initialize(ServiceProvider.StageDragSelector);
        }

        protected internal virtual void Initialize(GameStageContext context)
        {
            context.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.SuddenPlus))) {
                    var ratio = ConvertSuddenPlusToVisibleRangeCullingRatio(s.SuddenPlus);
                    VisibleRangeCullingRatio = ratio;
                    PerspectiveLinesRenderer.SetVisibleRangeCullingRatio(ratio);
                }
            });
        }

        public virtual void ApplyCameraTargetTexture(RenderTexture renderTexture)
        {
            if (PerspectiveCamera.targetTexture != renderTexture) {
                PerspectiveCamera.targetTexture = renderTexture;
            }
            var width = renderTexture.width;
            var height = renderTexture.height;
            var h = 9f / 16f * width / height;
            PerspectiveCamera.rect = new Rect(0f, 0f, 1f, h);
        }

        #region Value Convert

        // - ConvertXXX: pure method
        // - EvaluateXXX: related to states that can be controlled by user, eg. NoteFallSpeed
        // - RaycastXXX: raycast operation

        private float ConvertFallSpeedToPlaneSpeed(float noteFallSpeed)
            => 3 * Mathf.Pow(1.4f, noteFallSpeed);

        protected abstract float ConvertSuddenPlusToVisibleRangeCullingRatio(float suddenPlus);

        internal float EvaluateNoteActiveAheadTime(float noteSpeed)
            => NoteActiveAheadTime / noteSpeed;

        internal float EvaluateNoteActiveTime(INoteSpeed node)
            => node.Time - EvaluateNoteActiveAheadTime(node.Speed);

        internal float EvaluateNoteAppearAheadTime(float noteSpeed)
            => NoteAppearAheadTime / noteSpeed;

        internal float ConvertNotePositionToWorldX(float pos)
            => pos * Config.NotePosToWorldXFactor;

        internal float ConvertWorldXToNotePosition(float x)
            => x / Config.NotePosToWorldXFactor;

        internal float ConvertWorldZToNoteTime(float z, float speed)
            => z / speed / Config.NoteTimeToWorldZFactor;

        internal float ConvertNoteTimeToWorldZ(float time, float speed = 1f)
            => time * speed * Config.NoteTimeToWorldZFactor;

        internal float EvaluateNoteWorldZ(float time, float noteSpeed = 1f)
            => time * noteSpeed * ConvertFallSpeedToPlaneSpeed(_context.ActualNoteFallSpeed) * Config.NoteTimeToWorldZFactor;

        internal float EvaluateNoteLocalTime(float z, float noteSpeed = 1f)
            => z / noteSpeed / ConvertFallSpeedToPlaneSpeed(_context.ActualNoteFallSpeed) / Config.NoteTimeToWorldZFactor;

        internal (float X, float Z) EvaluateNoteWorldXZ(NoteCoord coord, float noteSpeed = 1f)
            => (ConvertNotePositionToWorldX(coord.Position), EvaluateNoteWorldZ(coord.Time, noteSpeed));

        // Perspective View

        /// <summary>
        /// Convert viewport point in panel view to local note coord
        /// </summary>
        internal bool TryEvaluatePerspectiveViewportPointToLocalNoteCoord(Vector2 perspectiveViewportPoint, float noteSpeed, out NoteCoord coord)
        {
            if (!IsInViewArea(perspectiveViewportPoint)) {
                coord = default;
                return false;
            }

            var raycastableVp = ConvertPerspectiveViewportPointToRaycastableViewportPoint(perspectiveViewportPoint);
            if (TryConvertRaycastableViewportPointToNotePlanePosition(raycastableVp, out var planePos)) {
                coord = new(
                    ConvertWorldXToNotePosition(planePos.X),
                    EvaluateNoteLocalTime(planePos.Z, noteSpeed));
                return true;
            }

            coord = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        /// <summary>
        /// The raycastable viewport is the bottom 16:9 part of the stage view
        /// </summary>
        private Vector2 ConvertPerspectiveViewportPointToRaycastableViewportPoint(Vector2 perspectiveViewportPoint)
        {
            return perspectiveViewportPoint with {
                y = perspectiveViewportPoint.y / PerspectiveCamera.rect.height
            };
        }

        protected bool TryConvertRaycastableViewportPointToNotePlanePosition(Vector2 raycastableViewportPoint, out (float X, float Z) notePlanePosition)
        {
            var ray = PerspectiveCamera.ViewportPointToRay(raycastableViewportPoint);
            if (NotePlane.Plane.Raycast(ray, out var distance)) {
                var hitPoint = ray.GetPoint(distance);
                notePlanePosition = (hitPoint.x, hitPoint.z);
                return true;
            }

            notePlanePosition = default;
            return false;
        }

        internal bool TryRaycastPerspectiveViewportPointToNote(Vector2 perspectiveViewportPoint, [MaybeNullWhen(false)] out GameStageNoteController note)
        {
            if (!IsInViewArea(perspectiveViewportPoint)) {
                note = default;
                return false;
            }

            var raycastableVp = ConvertPerspectiveViewportPointToRaycastableViewportPoint(perspectiveViewportPoint);
            var ray = PerspectiveCamera.ViewportPointToRay(raycastableVp);
            if (Physics.Raycast(ray, out var hitInfo)) {
                var c = hitInfo.collider;
                if (c != null && c.TryGetComponent<GameStageNoteRaycastingCollider>(out var collider)) {
                    note = collider.NoteController;
                    return true;
                }
            }

            note = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        internal bool TryConvertNotePlanePositionToRaycastableViewportPoint((float X, float Z) notePlanePosition, out Vector2 viewportPoint)
        {
            var y = NotePlane.ContentTransform.position.y;
            var camera = PerspectiveCamera;
            var position = new Vector3(notePlanePosition.X, y, notePlanePosition.Z);
            var vp = camera.WorldToViewportPoint(position);
            if (vp.z >= camera.nearClipPlane && vp.z <= camera.farClipPlane) {
                viewportPoint = vp;
                return true;
            }

            viewportPoint = default;
            return false;
        }

        #endregion
    }
}