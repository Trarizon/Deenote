#nullable enable

using Deenote.Contexts;
using Deenote.Core;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage.Foreground;
using Deenote.CoreB.Notification;
using Deenote.GameStage.Themes;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using Deenote.Library.Unity.UI;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed partial class PerspectiveViewPanelView : MonoBehaviour
    {
        private ProjectContext _projectContext;

        [SerializeField] AspectRatioFitter _aspectRatioFitter = default!;
        [SerializeField] RawImage _viewRawImage = default!;
        [SerializeField] IntegralSizeAspectRatioFitter _viewImageAspectRatioFitter = default!;

        [SerializeField] Transform _windowScreenParentTransform = default!;
        [SerializeField] Transform _fullScreenParentTransform = default!;
        [SerializeField] RectTransform _contentTransform = default!;
        [SerializeField] PointerHoveringTrigger _contentHoveringTrigger = default!;

        [SerializeField] GraphicRaycaster _raycaster = default!;

        private RenderTexture _viewRenderTexture = default!;

        public ForegroundPerspectiveViewUI StageForeground { get; private set; } = default!;

        public RenderTexture ViewRendererTexture => _viewRenderTexture;

        private FrameCachedNotifyingProperty<Vector2> _viewSize_bf = default!;
        public Vector2 ViewSize => _viewSize_bf.Value;

        private bool _isFullScreen_bf;
        public bool IsFullScreen => _isFullScreen_bf;

        public float AspectRatio
        {
            get => _aspectRatioFitter.aspectRatio;
            set {
                if (_aspectRatioFitter.aspectRatio == value)
                    return;
                _aspectRatioFitter.aspectRatio = value;
                _viewImageAspectRatioFitter.AspectRatio = value;
                AspectRatioChanged?.Invoke(value);
            }
        }

        public event Action<float>? AspectRatioChanged;

        public void SetIsFullScreen(bool full)
        {
            if (Utils.SetField(ref _isFullScreen_bf, full)) {
                if (full)
                    ApplicationManager.SetAspectRatio(AspectRatio, true);
                else
                    ApplicationManager.RecoverResolution();
                _contentTransform.SetParent(full ? _fullScreenParentTransform : _windowScreenParentTransform, false);
                _fullScreenParentTransform.gameObject.SetActive(full);
                _windowScreenParentTransform.gameObject.SetActive(!full);
            }
        }

        private void Awake()
        {
            _projectContext= MainSystem.Contexts.Project;

            InitAspectRatioController();

            MainSystem.GameStageThemeManager.ThemeLoaded += _OnStageLoaded;
            //MainSystem.GamePlayManager.StageLoaded += _OnStageLoaded;

            void InitAspectRatioController()
            {
                _viewRenderTexture = new RenderTexture(1280, 720, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None);
                _viewRawImage.texture = _viewRenderTexture;
                _viewRawImage.enabled = true;

                var imgTsfm = _viewRawImage.rectTransform;

                _viewSize_bf = new FrameCachedNotifyingProperty<Vector2>(GetViewSize, autoUpdate: true);
                _viewSize_bf.OnValueChanged += ResizeTargetTexture;

                Vector2 GetViewSize()
                {
                    var rect = _viewRawImage.rectTransform.rect;
                    return new Vector2(rect.width, rect.height);
                }

                void ResizeTargetTexture(Vector2 old, Vector2 size)
                {
                    _viewRenderTexture.Resize(MathUtils.RoundToInt(size));
                    MainSystem.GamePlayManager.Stage?.ApplyCameraTargetTexture(_viewRenderTexture);
                }
            }
        }

        private void Start()
        {
            _projectContext.RegisterPropertyChangedAndInvoke((s, e) =>
            {
                if (e.MatchProperty(nameof(s.CurrentChart))) {
                    _viewRawImage.enabled = s.CurrentChart is not null;
                }
            });
        }

        private void _OnStageLoaded(GameStageThemeEntry args)
        {
            args.Stage.ApplyCameraTargetTexture(_viewRenderTexture);

            var foreground = args.InstantiateUIAsync(_contentTransform);
            //var foreground = Instantiate(args.PerspectiveViewForegroundPrefab, _contentTransform);
            if (StageForeground != null) {
                Destroy(StageForeground.gameObject);
            }
            StageForeground = foreground;

            _raycaster.enabled = true;
        }

        public bool TryConvertScreenPointToViewportPoint(Vector2 screenPoint, out Vector2 viewportPoint)
        {
            var tsfm = _viewRawImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(tsfm, screenPoint, null, out var localPoint)) {
                viewportPoint = default;
                return false;
            }

            var tsfmrect = tsfm.rect;
            viewportPoint = new Vector2(
                localPoint.x / tsfmrect.width,
                localPoint.y / tsfmrect.height);
            return true;
        }

        #region Pointer

        /// <summary>
        /// InputManager requires this to judge if mouse actions should be enabled.
        /// </summary>
        public bool IsHovering => _contentHoveringTrigger.IsHovering;

        #endregion
    }
}