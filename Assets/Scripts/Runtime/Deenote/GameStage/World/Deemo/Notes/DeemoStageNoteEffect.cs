using Deenote.Core.GameStage;
using Deenote.Library;
using TriInspector;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements.Experimental;

namespace Deenote.GameStage.World.Deemo.Notes
{
    enum DeemoGameVersion
    {
        Deemo,
        DeemoV4,
    }

    [MovedFrom(true, sourceClassName: "DeemoGameStageNoteEffect")]
    internal class DeemoStageNoteEffect : MonoBehaviour
    {
        [SerializeField] DeemoGameStageNoteController _note;

        [Title("Sprites")]
        [SerializeField] Transform _explosionScaler;
        [SerializeField] SpriteRenderer _explosionSpriteRenderer;
        [SerializeField] Transform _circlewaveScaler;
        [SerializeField] SpriteRenderer _circlewaveSpriteRenderer;
        [SerializeField] Transform _shockwaveScaler;
        [SerializeField] SpriteRenderer _shockwaveSpriteRenderer;
        [SerializeField] Transform _glowScaler;
        [SerializeField] SpriteRenderer _glowSpriteRenderer;

        [Space]
        [SerializeField] Transform _holdingExplosionScaler;
        [SerializeField] SpriteRenderer _holdingExplosionSpriteRenderer;

        [Title("Configs")]
        [SerializeField] DeemoGameVersion _version;

        private int _currentExplosionSpriteIndex;
        private EffectKind _activeEffectKind;

        void Start()
        {
            _explosionSpriteRenderer.transform.localScale = _note._config.ExplosionScale * Vector3.one;
        }

        public void SetShockwaveColor(Color color)
        {
            _shockwaveSpriteRenderer.color = color;
        }

        public void SetActiveEffectKind(EffectKind kind)
        {
            if (Utils.SetField(ref _activeEffectKind, kind)) {
                _explosionSpriteRenderer.enabled = kind == EffectKind.Hit;
                _circlewaveSpriteRenderer.enabled = kind == EffectKind.Hit;
                _shockwaveSpriteRenderer.enabled = kind == EffectKind.Hit;
                _glowSpriteRenderer.enabled = kind == EffectKind.Hit;
                _holdingExplosionSpriteRenderer.enabled = kind == EffectKind.Holding;
            }
        }

        public void SetNoteSizeScaler(float value)
        {
            switch (_version) {
                case DeemoGameVersion.Deemo:
                    var scale = new Vector3(value, value, value);
                    _explosionScaler.localScale = scale;
                    _circlewaveScaler.localScale = scale;
                    _shockwaveScaler.localScale = scale;
                    _holdingExplosionScaler.localScale = scale;
                    break;
                case DeemoGameVersion.DeemoV4:
                    var scaleV4 = new Vector3(value, 1, 1);
                    _explosionScaler.localScale = scaleV4;
                    _circlewaveScaler.localScale = scaleV4;
                    _shockwaveScaler.localScale = scaleV4;
                    _holdingExplosionScaler.localScale = scaleV4;
                    _glowScaler.localScale = scaleV4;
                    break;
            }
        }

        public void SetHoldingEffect(float passedTime, float duration)
        {
            SetActiveEffectKind(EffectKind.Holding);
        }

        public void SetHitEffect(float passedTime)
        {
            SetActiveEffectKind(EffectKind.Hit);
            SetShockwave(passedTime);
            SetCirclewave(passedTime);
            SetExplosion(passedTime);
            SetGlow(passedTime);
        }

        private void SetShockwave(float time)
        {
            // 0.3333s

            const float GrowTime = 5f / 60f;
            const float FadeTime = 0.25f;
            var tsfm = _shockwaveSpriteRenderer.transform;

            float scale;
            if (time < GrowTime) {
                var t = time / GrowTime;
                scale = 1.5f * Easing.OutCubic(t);
            }
            else if (time < GrowTime + FadeTime) {
                var t = (0.25f - (time - GrowTime)) / 0.25f;
                scale = 1.5f * Easing.OutCubic(t);
            }
            else {
                scale = 0;
            }

            tsfm.WithLocalScaleY(scale * _note._config.ShockwaveScale);
        }

        private void SetCirclewave(float time)
        {
            // 0.5s
            const float Time1 = 1f / 6f;
            const float Time2 = 1f / 3f;

            Vector3 scale;
            float alpha;
            if (time < Time1) {
                var s = 1 + time / (17f / 60f);
                scale = new Vector3(s, s, 1);
                alpha = time / Time1;
            }
            else if (time < Time2) {
                float s = Mathf.Min(2f, 1f + time / (17f / 60f));
                scale = new Vector3(s, s, 1);
                alpha = Mathf.InverseLerp(Time2, Time1, time);
            }
            else {
                scale = Vector3.one;
                alpha = 0f;
            }

            _circlewaveSpriteRenderer.transform.localScale = scale * _note._config.CirclewaveScale;
            _circlewaveSpriteRenderer.WithColorAlpha(alpha);
        }

        private void SetExplosion(float time)
        {
            var config = _note._config;
            if (time < config.ExplosionAnimationDuration) {
                var index = Mathf.FloorToInt(time / config.ExplosionAnimationDuration * config.ExplosionSprites.Length);
                index = Mathf.Min(index, config.ExplosionSprites.Length);
                if (_currentExplosionSpriteIndex != index) {
                    if (index == 0) {
                        _explosionSpriteRenderer.sprite = null;
                    }
                    else {
                        _explosionSpriteRenderer.sprite = config.ExplosionSprites[index - 1];
                    }
                    _currentExplosionSpriteIndex = index;
                }
            }
            else {
                _explosionSpriteRenderer.sprite = null;
            }
        }

        private void SetGlow(float time)
        {
            // 0.91666666666667

            // The glow effect animation is related to frame count rather than time,
            // We use 60 fps as the frame count here.

            const float BaseScaleX = 0.5f;
            const float DeltaTime = 1f / 60f;

            float alpha;
            Vector2 scale;
            if (time < DeltaTime * 5) {
                alpha = time / (DeltaTime * 10);
                scale = new Vector2(
                    x: BaseScaleX + time / (DeltaTime * 100),
                    y: time / (DeltaTime * 10));
            }
            else if (time < DeltaTime * 55) {
                var t = 1 - (time - DeltaTime * 5) / (DeltaTime * 50);
                alpha = t * 0.5f;
                scale = new Vector2(
                    x: BaseScaleX + time / (DeltaTime * 100),
                    y: t * 0.5f);
            }
            else {
                alpha = 0;
                scale = new Vector2(BaseScaleX + 0.55f, y: 0f);
            }

            _glowSpriteRenderer.WithColorAlpha(alpha);
            _glowSpriteRenderer.transform.localScale = scale * _note._config.GlowScale;
        }

        public enum EffectKind
        {
            Holding = 1,
            Hit = 2,
        }

        void OnValidate()
        {
            _note ??= this.GetComponentInParent<DeemoGameStageNoteController>();
        }
    }
}
