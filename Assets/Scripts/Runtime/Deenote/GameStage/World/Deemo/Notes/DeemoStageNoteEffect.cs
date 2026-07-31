using Deenote.Core.GameStage;
using Deenote.Library;
using Deenote.Replica;
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
            _shockwaveSpriteRenderer.transform.WithLocalScaleY(
                _note._config.ShockwaveScale * DeemoReplica.CalcNoteShockwaveEffectScale(time));
        }

        private void SetCirclewave(float time)
        {
            var (scale, alpha) = DeemoReplica.CalcNoteCirclewaveEffect(time);

            _circlewaveSpriteRenderer.transform.localScale = new Vector3(scale, scale, 1) * _note._config.CirclewaveScale;
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

            var (scale, alpha) = DeemoReplica.CalcNoteGlowEffect(time);

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
