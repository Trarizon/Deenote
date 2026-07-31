using Deenote.Library;
using UnityEngine;

namespace Deenote.GameStage.World.Deemo
{
    internal sealed class DeemoStageJudgeLineEffect : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _breathingEffectSpriteRenderer;
        [SerializeField] Transform _hitEffectScaler;

        private float _breathingTime;
        private bool _breathingEnabled;
        public bool BreathingEnabled
        {
            get => _breathingEnabled;
            set {
                if (Utils.SetField(ref _breathingEnabled, value)) {
                    if (!value) {
                        _breathingTime = 0;
                    }
                }
            }
        }

        void Update()
        {
            if (BreathingEnabled) {
                _breathingTime += Time.deltaTime;
                SetBreathingEffect(_breathingTime);
            }
        }

        public void SetHitEffect(float? time)
        {
            if (time is not { } t) {
                _hitEffectScaler.WithLocalScaleY(0);
                return;
            }

            // The Animation is related to frame count rather than time
            // We use 60 fps as frame count here
            const float Interval = 1f / 60f;

            // First frame is 1, dec 0.02 per frame

            var size = 1 - 0.02f * (t / Interval);
            size = Mathf.Max(0, size);
            _hitEffectScaler.WithLocalScaleY(size);
        }

        private void SetBreathingEffect(float time)
        {
            // The animation is related to frame count rather than time
            // We use 60 fps as frame count here
            const float Interval = 1 / 60f;

            // 100 frames, inc by 0.01
            time %= Interval * 200;

            float alpha;
            if (time < Interval * 100) {
                alpha = time / (Interval * 100);
            }
            else {
                alpha = (Interval * 200 - time) / (Interval * 100);
            }
            _breathingEffectSpriteRenderer.color = new Color(1, 1, 1, alpha);
        }
    }
}
