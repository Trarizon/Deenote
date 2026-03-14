#nullable enable

using Deenote.Core.GameStage.Themes.Deemo;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Systems.Configurations;
using UnityEngine;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    internal sealed class DeemoGameStageNoteController : GameStageNoteController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRenderer = default!;
        [SerializeField] Transform _headTransform = default!;
        [SerializeField] Transform _holdBodyTransform = default!;
        [SerializeField] DeemoGameStageNoteEffect _effect = default!;

        [SerializeField] DeemoGameStageNoteConfig _config = default!;

        private Color _waveColor;

        private DeemoGameStageController? _deemoStage;
        private DeemoGameStageController Stage => _deemoStage ??= (DeemoGameStageController)_game.Stage!;

        public DeemoGameStageNoteConfig Config => _config;

        //protected override void SetNoteSpriteRendererAlpha(float alpha)
        //{
        //    _noteSpriteRenderer.WithColorAlpha(alpha);
        //}

        //protected override void SetHoldScaleY(float scaleY, bool isHolding)
        //{
        //    _holdBodySpriteRenderer.transform.WithLocalScaleY(scaleY);
        //    _holdBodySpriteRenderer.color = isHolding
        //        ? Stage.DeemoArgs.HoldingBodyColor
        //        : Color.white;
        //}

        //protected override void SetNoteSpriteColorRGB(Color color)
        //{
        //    _noteSpriteRenderer.WithColorRGB(color);
        //}

        protected override void OnPlayStateChanged(NotePlayState state)
        {
            switch (state) {
                case NotePlayState.Invisible:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(false);
                    _effect.gameObject.SetActive(false);
                    break;
                case NotePlayState.Fall:
                    _noteSpriteRenderer.gameObject.SetActive(true);
                    _holdBodySpriteRenderer.gameObject.SetActive(true);
                    _effect.gameObject.SetActive(false);
                    RefreshHighlightState();
                    break;
                case NotePlayState.Holding:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(true);
                    _effect.gameObject.SetActive(true);
                    _effect.SetActiveEffectKind(DeemoGameStageNoteEffect.EffectKind.Holding);
                    break;
                case NotePlayState.HitEffect:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(false);
                    _effect.gameObject.SetActive(true);
                    _effect.SetActiveEffectKind(DeemoGameStageNoteEffect.EffectKind.Hit);
                    break;
            }
        }

        private float _fadeInAlpha;
        private float _downplayAlpha;

        private float SpriteAlpha => _fadeInAlpha * _downplayAlpha;

        protected override void OnFallingProgressChanged(float timeToHit, float appearAheadTime)
        {
            var noteFadeEndTime = appearAheadTime * (1 - _plane.GameStage.Config.NoteFadeInRatio);
            _fadeInAlpha = Mathf.InverseLerp(appearAheadTime, noteFadeEndTime, timeToHit);
            _noteSpriteRenderer.WithColorAlpha(SpriteAlpha);
        }

        protected override void OnNoteHighlightChanged(NoteHighlightFlags flags)
        {
            var color = (flags & (NoteHighlightFlags.Selected | NoteHighlightFlags.Collided)) switch {
                NoteHighlightFlags.Selected => Config.SelectedHighlightColor,
                NoteHighlightFlags.Collided => Config.CollidedHighlightColor,
                NoteHighlightFlags.Selected | NoteHighlightFlags.Collided => Config.SelectedAndCollidedHighlightColor,
                _ => Color.white,
            };
            _downplayAlpha = flags.HasFlag(NoteHighlightFlags.Downplayed)
                ? Config.DownplayAlpha : 1f;
            color.a = SpriteAlpha;
            _noteSpriteRenderer.color = color;
        }

        protected override void OnNoteHeadKindChanged(NoteHeadKind kind)
        {
            var data = kind switch {
                NoteHeadKind.Swipe => Config.SwipeNoteSpriteData,
                NoteHeadKind.Slide => Config.SlideNoteSpriteData,
                NoteHeadKind.NoSound => _game.IsPianoNotesDistinguished ? Config.NoSoundNoteSpriteData : Config.ClickNoteSpriteData,
                NoteHeadKind.Click => Config.ClickNoteSpriteData,
                _ => Config.ClickNoteSpriteData,
            };

            _noteSpriteRenderer.sprite = data.Sprite;
            _noteSpriteRenderer.transform.localScale = new Vector3(data.Scale, data.Scale, data.Scale);
            _effect.SetShockwaveColor(data.WaveColor);
        }

        protected override void OnNoteIsHoldChanged(bool isHold)
        {
            //_holdBodySpriteRenderer.gameObject.SetActive(isHold);
        }

        protected override void OnHoldStatusChanged(NoteHoldingStatus status)
        {
            _holdBodySpriteRenderer.gameObject.SetActive(true);
            var scale = Stage.EvaluateNoteWorldZ(status.RestTime, NoteModel.Speed);
            _holdBodyTransform.WithLocalScaleYZ(scale, scale);
            _holdBodySpriteRenderer.color = status.IsHolding
                ? Config.HoldingBodyColor
                : Color.white;

            if (status.IsHolding) {
                _effect.SetHoldingEffect(status.PassedTime, status.Duration);
            }
        }
        protected override void OnHitEffectTimeChanged(float time)
        {
            _effect.SetHitEffect(time);
        }
        protected override void OnNoteSizeChanged(float size)
        {
            _headTransform.localScale = new Vector3(size, 1f, 1f);
            _effect.SetNoteSizeScaler(size);
            _holdBodyTransform.WithLocalScaleX(size);
        }
    }
}