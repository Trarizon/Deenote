using System;
using UnityEngine;

namespace Deenote.Systems
{
    public class ResolutionManager
    {
        public event Action<Vector2Int>? ResolutionChanged;
        public event Action<Vector2Int>? ActualResolutionChanged;

        internal ResolutionManager()
        {
            _resolution = new(Screen.width, Screen.height);
        }

        private Vector2Int _resolution;
        private float? _forceAspectRatio;

        public Vector2Int Resolution
        {
            get => _resolution;
            set {
                if (_resolution != value) {
                    _resolution = value;
                    ResetScreenResolution();
                    ResolutionChanged?.Invoke(value);
                }
            }
        }

        public Vector2Int ActualResolution
        {
            get => new(Screen.width, Screen.height);
        }

        public void SetForceAspectRatio(float ratio)
        {
            _forceAspectRatio = ratio;
            ResetScreenResolution();
        }

        public void RecoverForceAspectRatio()
        {
            _forceAspectRatio = null;
            ResetScreenResolution();
        }

        private void ResetScreenResolution()
        {
            var before = new Vector2Int(Screen.width, Screen.height);

            var height = Resolution.y;
            var width = _forceAspectRatio is { } ratio ? (int)(height * ratio) : Resolution.x;

            if (before != new Vector2Int(width, height)) {
                Screen.SetResolution(width, height, false);
                ActualResolutionChanged?.Invoke(new(Screen.width, Screen.height));
            }
        }
    }
}