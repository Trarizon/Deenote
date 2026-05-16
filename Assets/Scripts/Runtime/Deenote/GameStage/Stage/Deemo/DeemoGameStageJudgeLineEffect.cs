#nullable enable

using Deenote.Library;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.Stage.Deemo
{
    [MovedFrom("Deenote.Core.GameStage.Themes.Deemo")]
    [ExecuteAlways]
    internal sealed class DeemoGameStageJudgeLineEffect : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _breathingEffectSpriteRenderer = default!;
        [SerializeField] Transform _hitEffectScaler = default!;

        private float _time;

        private bool _breathingEnabled;

        public bool BreathingEnabled
        {
            get => _breathingEnabled;
            set {
                if (Utils.SetField(ref _breathingEnabled, value)) {
                    if (!value) {
                        _time = 0;
                        SetBreathingEffect();
                    }
                }
            }
        }

        private void Update()
        {
            // The animation is related to frame count rather than time
            // We use 60 fps as frame count here
            const float Interval = 1f / 60f;

            _time += Time.deltaTime;
            _time %= Interval * 200;
            SetBreathingEffect();
        }

        public void SetHitEffect(float? time)
        {
            if (time is not { } t){
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

        private void SetBreathingEffect()
        {
            const float Interval = 1 / 60f;

            // 100 frames, inc by 0.01

            float alpha;
            if (_time < Interval * 100) {
                alpha = _time / (Interval * 100);
            }
            else {
                alpha = (Interval * 200 - _time) / (Interval * 100);
            }
            _breathingEffectSpriteRenderer.color = new Color(1, 1, 1, alpha);
        }
    }
}