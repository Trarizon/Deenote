#nullable enable

using Deenote.Core.GameStage.Themes.Deemo;
using Deenote.Library;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    internal class DeemoGameStageNoteEffect : MonoBehaviour
    {
        [SerializeField] DeemoGameStageNoteController _note = default!;

        [Header("Waves")]
        [SerializeField] protected Transform _explosionScaler = default!;
        [SerializeField] protected SpriteRenderer _explosionSpriteRenderer = default!;
        [SerializeField] protected Transform _circlewaveScaler = default!;
        [SerializeField] protected SpriteRenderer _circlewaveSpriteRenderer = default!;
        [SerializeField] protected Transform _shockwaveScaler = default!;
        [SerializeField] protected SpriteRenderer _shockwaveSpriteRenderer = default!;
        [SerializeField] protected SpriteRenderer _glowSpriteRenderer = default!;

        [SerializeField] protected Transform _holdingExplosionScaler = default!;
        [SerializeField] protected SpriteRenderer _holdingExplosionSpriteRenderer = default!;

        private int _currentExplosionSpriteIndex;

        private EffectKind _activeEffectKindCache;

        private void Start()
        {
            _explosionSpriteRenderer.transform.localScale = _note.Config.ExplosionScale * Vector3.one;
        }

        public void SetShockwaveColor(Color value)
        {
            _shockwaveSpriteRenderer.color = value;
        }

        public void SetActiveEffectKind(EffectKind kind)
        {
            if (Utils.SetField(ref _activeEffectKindCache, kind)) {
                _explosionSpriteRenderer.enabled = kind == EffectKind.Hit;
                _circlewaveSpriteRenderer.enabled = kind == EffectKind.Hit;
                _shockwaveSpriteRenderer.enabled = kind == EffectKind.Hit;
                _glowSpriteRenderer.enabled = kind == EffectKind.Hit;
                _holdingExplosionSpriteRenderer.enabled = kind == EffectKind.Holding;
            }
        }

        public virtual void SetNoteSizeScaler(float value)
        {
            var scale = new Vector3(value, value, value);
            _explosionScaler.localScale = scale;
            _circlewaveScaler.localScale = scale;
            _shockwaveScaler.localScale = scale;
            _holdingExplosionScaler.localScale = scale;
        }

        public void SetHoldingEffect(float passedTime,float duration)
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
            // 0.33333333333333s

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

            tsfm.WithLocalScaleY(scale * _note.Config.ShockwaveScale);
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

            _circlewaveSpriteRenderer.transform.localScale = scale * _note.Config.CirclewaveScale;
            _circlewaveSpriteRenderer.WithColorAlpha(alpha);
        }

        private void SetExplosion(float time)
        {
            var config = _note.Config;
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
            _glowSpriteRenderer.transform.localScale = scale * _note.Config.GlowScale;
        }

        private void OnValidate()
        {
            _explosionSpriteRenderer.transform.localScale = _note.Config.ExplosionScale * Vector3.one;
        }

        public enum EffectKind
        {
            Holding,
            Hit,
        }
    }
}