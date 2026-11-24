using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.CoreB.Notification;
using Deenote.Entities;
using Deenote.GamePlay;
using Deenote.GameStage.Grids;
using Deenote.GameStage.UI;
using Deenote.GameStage.World;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using TriInspector;
using UnityEngine;

namespace Deenote.GameStage
{
    public abstract class GameStageController : MonoBehaviour, INotifyPropertyChanged<GameStageController?>
    {
        [SerializeField] Camera _perspectiveCamera;
        [SerializeField] PerspectiveLinesRenderer _perspectiveLineRenderer = default!;
        [SerializeField] Transform _notePanelTransform = default!;
        [SerializeField] GameStageNotePlane _notePlane;
        [SerializeField] SelectionAreaRect _selectionAreaRect = default!;
        [SerializeField] Transform _noteIndicatorPanelTransform = default!;
        [SerializeField] RectTransform _noteDragSelectionPanelTransform = default!;

        protected internal Camera PerspectiveCamera => _perspectiveCamera;

        public PerspectiveLinesRenderer PerspectiveLinesRenderer => _perspectiveLineRenderer;

        /// <summary>
        /// The parent transform of instantiated notes
        /// </summary>
        public Transform NotePanelTransform => _notePanelTransform;

        public Transform NoteIndicatorPanelTransform => _noteIndicatorPanelTransform;

        public GameStageNotePlane NotePlane => _notePlane;
        public SelectionAreaRect SelectionAreaRect => _selectionAreaRect;

        [Title("Config")]
        [SerializeField] GameStageConfig _config;
        [SerializeField] GridLineConfig _gridLineConfig;

        public GameStageConfig Config => _config;
        public GridLineConfig GridLineConfig => _gridLineConfig;

        protected GamePlayManager _manager = default!;

        private bool _isStageEffectOn_bf;
        private float _visibleRangePercentage_bf;

        // public bool IsStageEffectOn
        // {
        //     get => _isStageEffectOn_bf;
        //     set {
        //         if (Utils.SetField(ref _isStageEffectOn_bf, value)) {
        //             OnIsStageEffectOnChanged(value);
        //         }
        //     }
        // }
        public float VisibleRangePercentage
        {
            get => _visibleRangePercentage_bf;
            set {
                if (Utils.SetField(ref _visibleRangePercentage_bf, value)) {
                    OnVisibleRangePercentageChanged(value);
                    PropertyChanged?.Invoke(this, nameof(VisibleRangePercentage));
                }
            }
        }

        protected GamePlayManager2 _gamePlayManager = default!;

        public GameStageManager StageManager { get; private set; } = default!;

        public event Action<GameStageController?, PropertyChangedEventArgs>? PropertyChanged;

        protected internal virtual void OnInstantiate(GameStageManager stageManager)
        {
            StageManager = stageManager;
            _gamePlayManager = App.GamePlayManager;
            StageManager.ForegroundViewPanel.ViewSizeChanged += _OnViewSizeChanged;
            _OnViewSizeChanged(StageManager.ForegroundViewPanel);

            _selectionAreaRect.OnInstantiate(_gamePlayManager, stageManager, App.EditingManager.DragSelector);

            StageManager.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.Match(nameof(s.SuddenPlus))) {
                    var x = s.ConvertCoordPositionToWorldX(0f);
                    var maxZ = s.ConvertCoordTimeToWorldZ(s.StandardNoteActiveAheadTime);
                    var minZ = s.ConvertCoordTimeToWorldZ(0f);
                    var maxPos = _notePlane.GetWorldPosition(x, maxZ);
                    var minPos = _notePlane.GetWorldPosition(x, minZ);

                    var camera = PerspectiveCamera;
                    var maxVp = camera.WorldToViewportPoint(maxPos);
                    var minVp = camera.WorldToViewportPoint(minPos);

                    var spVp = new Vector2(maxVp.x, Mathf.Lerp(maxVp.y, minVp.y, s.SuddenPlus));
                    var ray = camera.ViewportPointToRay(spVp);
                    _notePlane.TryRaycast(ray, out var hitPoint);

                    var res = Mathf.InverseLerp(minZ, maxZ, hitPoint.z);
                    App.Logger.LogTrace($"VisibleRangePercentage: {res}");
                    VisibleRangePercentage = res;
                }
                if (e.Match(nameof(s.IsStageEffectOn))) {
                    OnIsStageEffectOnChanged(s.IsStageEffectOn);
                }
            }).UnregisterWhenGameObjectDestroyed(this);
        }

        void OnDestroy()
        {
            StageManager.ForegroundViewPanel.ViewSizeChanged -= _OnViewSizeChanged;
        }

        private void _OnViewSizeChanged(IGameStagePerspectiveViewPanel panel)
        {
            var rt = panel.ViewRendererTexture;
            if (_perspectiveCamera.targetTexture != rt) {
                _perspectiveCamera.targetTexture = rt;
            }
            OnRenderedViewChanged(rt);
        }

        internal void SetSelectionPanelRect(NoteCoord startCoord, NoteCoord endCoord)
        {
            var (xMin, zMin) = StageManager.ConvertCoordToWorld(startCoord - new NoteCoord(0, _gamePlayManager.CurrentTime));
            var (xMax, zMax) = StageManager.ConvertCoordToWorld(endCoord - new NoteCoord(0, _gamePlayManager.CurrentTime));

            _noteDragSelectionPanelTransform.gameObject.SetActive(true);
            _noteDragSelectionPanelTransform.offsetMin = new(xMin, zMin);
            _noteDragSelectionPanelTransform.offsetMax = new(xMax, zMax);
        }

        internal void SetSelectionPanelRectInvisible()
        {
            _noteDragSelectionPanelTransform.gameObject.SetActive(false);
            return;
        }

        protected virtual void OnIsStageEffectOnChanged(bool value) { }
        protected virtual void OnVisibleRangePercentageChanged(float value)
        {
            var cutoff = Mathf.Lerp(
                StageManager.ConvertCoordTimeToWorldZ(0),
                StageManager.ConvertCoordTimeToWorldZ(StageManager.StandardNoteActiveAheadTime),
                value
            );
            _perspectiveLineRenderer.SetCutoffAndFadeIn(cutoff, Config.GridLineFadeInRatio);
        }
        protected virtual void OnRenderedViewChanged(RenderTexture renderTexture) { }

        #region Perspective Converters

        internal Vector2 ConvertPerspectiveViewportPointToRaycastingViewportPoint(Vector2 perspectiveViewPanelViewportPoint)
        {
            var camera = PerspectiveCamera;
            return perspectiveViewPanelViewportPoint with {
                y = perspectiveViewPanelViewportPoint.y / camera.rect.height
            };
        }

        internal bool TryRaycastRaycastingViewportPointToNote(Vector2 raycastingViewportPoint, [NotNullWhen(true)] out GameStageNoteController? note)
        {
            var camera = PerspectiveCamera;
            var ray = camera.ViewportPointToRay(raycastingViewportPoint);
            if (Physics.Raycast(ray, out var hitInfo)) {
                var c = hitInfo.collider;
                if (c != null && c.TryGetComponent<GameStageNoteRaycastingCollider>(out var collider)) {
                    note = collider.Note;
                    return true;
                }
            }

            note = null;
            return false;
        }

        [Obsolete]
        internal bool TryRaycastRaycastingViewportPointToWarning(Vector2 raycastingViewportPoint,
            [NotNullWhen(true)] out GameStageSpeedChangeWarningNoteController? warning)
        {
            var camera = PerspectiveCamera;
            var ray = camera.ViewportPointToRay(raycastingViewportPoint);
            if (Physics.Raycast(ray, out var hitInfo)) {
                var c = hitInfo.collider;
                if (c != null) {
                    if (c.TryGetComponent<SpeedChangeWarningRaycastingCollider>(out var collider)
                        && collider.Note is not null) {
                        warning = collider.Note;
                        return true;
                    }
                }
            }

            warning = null;
            return false;
        }

        [Obsolete]
        internal bool TryConvertNotePanelPositionToRaycastingViewportPoint((float X, float Z) notePanelPosition, out Vector2 viewportPoint)
        {
            var y = _notePanelTransform.position.y;
            var camera = PerspectiveCamera;
            var position = new Vector3(notePanelPosition.X, y, notePanelPosition.Z);
            var vp = camera.WorldToViewportPoint(position);

            if (vp.z >= camera.nearClipPlane && vp.z <= camera.farClipPlane) {
                viewportPoint = vp;
                return true;
            }
            viewportPoint = default;
            return false;
        }

        [Obsolete]
        internal bool TryConvertRaycastingViewportPointToNotePanelPosition(Vector2 raycastingViewportPoint, out (float X, float Z) notePanelPosition)
        {
            var camera = PerspectiveCamera;
            var ray = camera.ViewportPointToRay(raycastingViewportPoint);
            var plane = new Plane(_notePanelTransform.up, _notePanelTransform.position);
            if (plane.Raycast(ray, out var distance)) {
                var hitPoint = ray.GetPoint(distance);
                notePanelPosition = (hitPoint.x, hitPoint.z);
                return true;
            }
            notePanelPosition = default;
            return false;
        }

        internal Ray GetRaycastingViewportRay(Vector2 raycastingViewportPoint)
        {
            var camera = PerspectiveCamera;
            return camera.ViewportPointToRay(raycastingViewportPoint);
        }

        #endregion
    }
}