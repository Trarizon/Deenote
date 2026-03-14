#nullable enable

using Deenote.Entities.Models;
using UnityEngine;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    public sealed class DeemoPlacementNoteIndicatorController : PlacementNoteIndicatorController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRender = default!;
        [SerializeField] Transform _headTransform = default!;
        [SerializeField] Transform _holdBodyTransform = default!;

        [SerializeField] DeemoGameStageNoteConfig _config;

        protected internal override void Refresh()
        {
            var stage = GameStage;
            var game = GameStage.GamePlay;

            //var args = stage.Args;
            var prefab = NotePrototype switch {
                { Kind: NoteModel.NoteKind.Swipe } => _config.SwipeNoteSpriteData,
                { Kind: NoteModel.NoteKind.Slide } => _config.SlideNoteSpriteData,
                { HasSounds: true } => _config.ClickNoteSpriteData,
                _ => game.IsPianoNotesDistinguished
                    ? _config.NoSoundNoteSpriteData
                    : _config.ClickNoteSpriteData,
            };

            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.transform.localScale = new Vector3(prefab.Scale, prefab.Scale, prefab.Scale);
            _headTransform.localScale = new Vector3(NotePrototype.Size, 1f, 1f);

            if (NotePrototype.IsSlide && NotePrototype.NextLink is not null) {
                var (tox, toz) = stage.EvaluateNoteWorldXZ(NotePrototype.NextLink.PositionCoord - NotePrototype.PositionCoord);
                _linkLineEndOffset = new Vector2(tox, toz);
            }
            else {
                _linkLineEndOffset = null;
            }

            if (NotePrototype.IsHold) {
                _holdBodySpriteRender.gameObject.SetActive(true);
                var scale = stage.EvaluateNoteWorldZ(NotePrototype.Duration, NotePrototype.Speed);
                _holdBodyTransform.localScale = new Vector3(NotePrototype.Size, scale, scale);
            }
            else {
                _holdBodySpriteRender.gameObject.SetActive(false);
            }
        }
    }
}