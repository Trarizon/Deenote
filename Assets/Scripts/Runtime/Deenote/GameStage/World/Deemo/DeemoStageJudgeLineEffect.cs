using Deenote.Library;
using Deenote.Replica;
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

            _hitEffectScaler.WithLocalScaleY(DeemoReplica.CalcJudgeLineHitEffectScale(t));
        }

        private void SetBreathingEffect(float time)
        {
            _breathingEffectSpriteRenderer.color = new Color(1, 1, 1, DeemoReplica.CalcJudgeLineBreathingEffectAlpha(time));
        }
    }
}
