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
                { Kind: NoteModel.NoteKind.Swipe } => _config.SwipeNoteSpriteData,
                { Kind: NoteModel.NoteKind.Slide } => _config.SlideNoteSpriteData,
                { HasSounds: true } => _config.ClickNoteSpriteData,
                _ when _game.IsPianoNotesDistinguished => _config.NoSoundNoteSpriteData,
                _ => _config.ClickNoteSpriteData,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.transform.localScale = Vector3.one * prefab.Scale;
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
                { Kind: NoteModel.NoteKind.Swipe } => _config.SwipeNoteSpriteData,
                { Kind: NoteModel.NoteKind.Slide } => _config.SlideNoteSpriteData,
                { HasSounds: true } => _config.ClickNoteSpriteData,
                _ when _game.IsPianoNotesDistinguished => _config.NoSoundNoteSpriteData,
                _ => _config.ClickNoteSpriteData,
            };
            _headTransform.localScale = new Vector3(NoteModel.Size, 1f, 1f);

            if (NoteModel.IsHold) {
                _holdBodyTransform.WithLocalScaleX(NoteModel.Size);
            }

            _noteEffect.SetNoteSizeScaler(NoteModel.Size);
        }

        protected override void SetNoteSpriteRendererAlpha(float alpha)
        {
            _noteSpriteRenderer.WithColorAlpha(alpha);
        }

        protected override void SetHoldScaleY(float scaleY, bool isHolding)
        {
            _holdBodyTransform.WithLocalScaleY(scaleY);
            _holdBodySpriteRenderer.color = isHolding
                ? _config.HoldingColor
                : Color.white;

            if (isHolding) {
                _noteEffect.SetHoldingEffect(0, NoteModel.Duration);
            }
        }

        protected override void SetNoteSpriteEditorStatus(bool selected, bool collided)
        {
            _noteSpriteRenderer.WithColorRGB((selected, collided) switch {
                (true, true) => _config.SelectedAndCollidedHighlightColor,
                (true, false) => _config.SelectedHighlightColor,
                (false, true) => _config.CollidedHighlightColor,
                _ => Color.white,
            });
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
                    _noteEffect.gameObject.SetActive(false);
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