#nullable enable

using Deenote.Entities.Models;
using Deenote.GameStage.World.Deemo.Notes;
using Deenote.Library;
using TriInspector;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class DeemoGameStageNoteController : GameStageNoteController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRenderer = default!;
        [SerializeField] Transform _headTransform;
        [SerializeField] Transform _holdBodyTransform;
        [SerializeField] DeemoStageNoteEffect _noteEffect;

        [Title("Config")]
        [SerializeField] internal DeemoStageNoteConfig _config = default!;

        private Color _waveColor;

        private DeemoGameStageController? _deemoStage;
        private DeemoGameStageController Stage => _deemoStage ??= (DeemoGameStageController)_game.Stage!;

        protected override void SetHoldingHitEffect()
        {
            _game.AssertStageLoaded();

            var stage = _game.Stage;

            //ref readonly var prefabs = ref stage.Args.HoldSpritePrefab;
            //ref readonly var effectPrefab = ref stage.Args.HitEffectSpritePrefabs;
            //_holdingExplosionSpriteRenderer.sprite = effectPrefab.HoldingExplosion;
        }

        protected override void SetHitEffect(float time)
        {
            _noteEffect.SetHitEffect(time);
        }

        protected override void SetNoteSprite()
        {
            _game.AssertStageLoaded();

            var prefab = NoteModel switch {
                { Kind: NoteModel.NoteKind.Swipe } => _game.Stage.Args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => _game.Stage.Args.SlideNoteSpritePrefab,
                { HasSounds: true } => _game.Stage.Args.BlackNoteSpritePrefab,
                _ when _game.IsPianoNotesDistinguished => _game.Stage.Args.NoSoundNoteSpritePrefab,
                _ => _game.Stage.Args.BlackNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _waveColor = prefab.WaveColor;

            if (NoteModel.IsHold) {
                _holdBodySpriteRenderer.gameObject.SetActive(true);
            }
            else {
                _holdBodySpriteRenderer.gameObject.SetActive(false);
            }

            _noteEffect.SetShockwaveColor(prefab.WaveColor);
        }

        protected override void SetNoteSize()
        {
            _game.AssertStageLoaded();

            var prefab = NoteModel switch {
                { Kind: NoteModel.NoteKind.Swipe } => _game.Stage.Args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => _game.Stage.Args.SlideNoteSpritePrefab,
                { HasSounds: true } => _game.Stage.Args.BlackNoteSpritePrefab,
                _ when _game.IsPianoNotesDistinguished => _game.Stage.Args.NoSoundNoteSpritePrefab,
                _ => _game.Stage.Args.BlackNoteSpritePrefab,
            };
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(NoteModel.Size, 1f, 1f) * prefab.Scale;

            ref readonly var hiteffectPrefab = ref _game.Stage.Args.HitEffectSpritePrefabs;
            var explosionEffectScale = NoteModel.Size * hiteffectPrefab.ExplosionScale * Vector3.one;
            explosionEffectScale.y *= Stage.DeemoArgs.HoldingExplosionScaleY;

            if (NoteModel.IsHold) {
                ref readonly var holdPrefab = ref _game.Stage.Args.HoldSpritePrefab;
                _holdBodySpriteRenderer.transform.WithLocalScaleX(NoteModel.Size * holdPrefab.ScaleX);
                // Scale.y is set when time changed
            }

            _noteEffect.SetNoteSizeScaler(NoteModel.Size);
        }

        protected override void SetNoteSpriteRendererAlpha(float alpha)
        {
            _noteSpriteRenderer.WithColorAlpha(alpha);
        }

        protected override void SetHoldScaleY(float scaleY, bool isHolding)
        {
            _holdBodySpriteRenderer.transform.WithLocalScaleY(scaleY);
            _holdBodySpriteRenderer.color = isHolding
                ? Stage.DeemoArgs.HoldingBodyColor
                : Color.white;

            if (isHolding) {
                _noteEffect.SetHoldingEffect(0, NoteModel.Duration);
            }
        }

        protected override void SetNoteSpriteColorRGB(Color color)
        {
            _noteSpriteRenderer.WithColorRGB(color);
        }

        protected override void OnStateChanged(NoteDisplayState state)
        {
            switch (state) {
                case NoteDisplayState.Invisible:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(false);
                    _noteEffect.gameObject.SetActive(false);
                    break;
                case NoteDisplayState.Fall:
                    _noteSpriteRenderer.gameObject.SetActive(true);
                    _holdBodySpriteRenderer.gameObject.SetActive(true);
                    RefreshColoring();
                    _noteEffect.gameObject.SetActive(true);
                    break;
                case NoteDisplayState.Holding:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(true);
                    _noteEffect.gameObject.SetActive(true);
                    _noteEffect.SetActiveEffectKind(DeemoStageNoteEffect.EffectKind.Holding);
                    break;
                case NoteDisplayState.HitEffect:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(false);
                    _noteEffect.gameObject.SetActive(true);
                    _noteEffect.SetActiveEffectKind(DeemoStageNoteEffect.EffectKind.Hit);
                    break;
            }
        }
    }
}