#nullable enable

using Deenote.Entities;
using Deenote.GameStage;
using Deenote.GameStage.Grids;
using Deenote.GameStage.World;
using Deenote.Library;
using System.Diagnostics.CodeAnalysis;
using TriInspector;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public abstract class GameStageController : MonoBehaviour
    {
        [SerializeField] GameStagePerspectiveCamera _perspectiveCamera = default!;
        [SerializeField] PerspectiveLinesRenderer _perspectiveLineRenderer = default!;
        [SerializeField] Transform _notePanelTransform = default!;
        [SerializeField] Transform _noteIndicatorPanelTransform = default!;
        [SerializeField] RectTransform _noteDragSelectionPanelTransform = default!;
        [SerializeField] Material _holdBodyCullMaterial = default!;

        public GameStagePerspectiveCamera PerspectiveCamera => _perspectiveCamera;

        public PerspectiveLinesRenderer PerspectiveLinesRenderer => _perspectiveLineRenderer;

        /// <summary>
        /// The parent transform of instantiated notes
        /// </summary>
        public Transform NotePanelTransform => _notePanelTransform;

        public Transform NoteIndicatorPanelTransform => _noteIndicatorPanelTransform;

        public GameStageNotePlaneController NotePlane => _notePanelTransform.GetComponent<GameStageNotePlaneController>();

        [Title("Config")]
        [SerializeField] GameStageConfig _config;
        [SerializeField] GridLineConfig _gridLineConfig;

        public GameStageConfig Config => _config;
        public GridLineConfig GridLineConfig => _gridLineConfig;

        protected GameStageManager _stage = default!;

        private bool _isStageEffectOn_bf;
        private float _visibleRangePercentage_bf;

        public bool IsStageEffectOn
        {
            get => _isStageEffectOn_bf;
            set {
                if (Utils.SetField(ref _isStageEffectOn_bf, value)) {
                    OnIsStageEffectOnChanged(value);
                }
            }
        }
        public float VisibleRangePercentage
        {
            get => _visibleRangePercentage_bf;
            set {
                if (Utils.SetField(ref _visibleRangePercentage_bf, value)) {
                    OnVisibleRangePercentageChanged(value);
                }
            }
        }

        private static readonly int HoldCullMaxZPropertyId = Shader.PropertyToID("_CullMaxZ");

        protected internal virtual void OnInstantiate(GameStageManager stage)
        {
            _stage = stage;
            _stage.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GameStageManager.SuddenPlus))
                    VisibleRangePercentage = _stage.VisibleRangePercentage;
                else if (e.PropertyName == nameof(GameStageManager.IsStageEffectOn))
                    IsStageEffectOn = _stage.IsStageEffectOn;
            };
            IsStageEffectOn = _stage.IsStageEffectOn;
            VisibleRangePercentage = _stage.VisibleRangePercentage;
            _perspectiveLineRenderer.OnInstantiate(_stage);
        }

        internal void SetSelectionPanelRect(NoteCoord startCoord, NoteCoord endCoord)
        {
            var (xMin, zMin) = _stage.ConvertNoteCoordToWorldPosition(startCoord - new NoteCoord(0, _stage.MusicTime));
            var (xMax, zMax) = _stage.ConvertNoteCoordToWorldPosition(endCoord - new NoteCoord(0, _stage.MusicTime));

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
            var time = _stage.StageNoteActiveAheadTime * value;
            var z = _stage.ConvertNoteCoordTimeToWorldZ(time);
            _holdBodyCullMaterial.SetFloat(HoldCullMaxZPropertyId, z);
        }

        #region Perspective Converters

        internal Vector2 ConvertPerspectiveViewportPointToRaycastingViewportPoint(Vector2 perspectiveViewPanelViewportPoint)
        {
            var camera = _perspectiveCamera.Camera;
            return perspectiveViewPanelViewportPoint with {
                y = perspectiveViewPanelViewportPoint.y / camera.rect.height
            };
        }

        internal bool TryRaycastRaycastingViewportPointToNote(Vector2 raycastingViewportPoint, [NotNullWhen(true)] out GameStageNoteController? note)
        {
            var camera = _perspectiveCamera.Camera;
            var ray = camera.ViewportPointToRay(raycastingViewportPoint);
            if (Physics.Raycast(ray, out var hitInfo)) {
                var c = hitInfo.collider;
                if (c != null && c.TryGetComponent<GameStageNoteRaycastingCollider>(out var collider)) {
                    note = collider.NoteController;
                    return true;
                }
            }

            note = null;
            return false;
        }

        internal bool TryConvertNotePanelPositionToRaycastingViewportPoint((float X, float Z) notePanelPosition, out Vector2 viewportPoint)
        {
            var y = _notePanelTransform.position.y;
            var camera = _perspectiveCamera.Camera;
            var position = new Vector3(notePanelPosition.X, y, notePanelPosition.Z);
            var vp = camera.WorldToViewportPoint(position);

            if (vp.z >= camera.nearClipPlane && vp.z <= camera.farClipPlane) {
                viewportPoint = vp;
                return true;
            }
            viewportPoint = default;
            return false;
        }

        internal bool TryConvertRaycastingViewportPointToNotePanelPosition(Vector2 raycastingViewportPoint, out (float X, float Z) notePanelPosition)
        {
            var camera = _perspectiveCamera.Camera;
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

        #endregion
    }
}