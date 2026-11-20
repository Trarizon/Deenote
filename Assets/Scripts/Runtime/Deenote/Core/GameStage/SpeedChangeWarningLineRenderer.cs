using Deenote.GameStage;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    /// <summary>
    /// Draws the time line of a speed change warning through
    /// <see cref="PerspectiveLinesRenderer"/> so that it is rendered in the same way as time grids.
    /// </summary>
    internal sealed class SpeedChangeWarningLineRenderer : MonoBehaviour
    {
        [SerializeField] private float _lineStartPosition = -3f;
        [SerializeField] private float _lineEndPosition = 2f;

        private GameStageController _stage = default!;
        private float _localTime;
        private float _speed = 1f;
        private float _alpha = 1f;
        private bool _visible;

        internal void OnInstantiate(GameStageController stage)
        {
            _stage = stage;
            _stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        private void OnDestroy()
        {
            if (_stage is not null) {
                _stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
            }
        }

        internal void SetTime(float localTime, float speed, float alpha)
        {
            _localTime = localTime;
            _speed = speed;
            _alpha = alpha;
        }

        internal void SetVisible(bool visible) => _visible = visible;

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (!_visible)
                return;

            if (_alpha <= 0)
                return;

            var z = _stage.StageManager.ConvertCoordTimeToWorldZ(_localTime, _speed, true);
            var xl = _stage.StageManager.ConvertCoordPositionToWorldX(_lineStartPosition);
            var xr = _stage.StageManager.ConvertCoordPositionToWorldX(_lineEndPosition);
            var args = _stage.GridLineConfig;
            collector.AddLine(
                new Vector2(xl, z),
                new Vector2(xr, z),
                args.WarningLineData.Color with { a = args.WarningLineData.Color.a * _alpha },
                args.WarningLineData.Width);
        }
    }
}
