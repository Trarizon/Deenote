#nullable enable

using Deenote.Entities.Models;
using Deenote.GameStage.World.Deemo.Notes;
using Deenote.CoreB.Unity;
using TriInspector;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class DeemoPlacementNoteIndicatorController : PlacementNoteIndicatorController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRender = default!;
        [SerializeField] Transform _headTransform;
        [SerializeField] Transform _holdBodyTransform;

        [Title("Configs")]
        [SerializeField] internal DeemoStageNoteConfig _config;

        protected internal override void Refresh()
        {
            _placer._editor._game.AssertStageLoaded();

            var game = _placer._editor._game;
            var stage = _placer._editor._game.Stage;
            var prefab = NotePrototype switch {
                { Kind: NoteModel.NoteKind.Swipe } => _config.SwipeNoteSpriteData,
                { Kind: NoteModel.NoteKind.Slide } => _config.SlideNoteSpriteData,
                { HasSounds: true } => _config.ClickNoteSpriteData,
                _ => App.GameStageManager.IsPianoNotesDistinguished
                    ? _config.NoSoundNoteSpriteData
                    : _config.ClickNoteSpriteData,
            };

            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.transform.localScale = prefab.Scale * Vector3.one;
            _headTransform.localScale = new Vector3(NotePrototype.Size, 1f, 1f);

            if (NotePrototype.IsSlide && NotePrototype.NextLink is not null) {
                // TODO: 不确定，疑似是bug，convert传coord delta貌似没有意义，可能是indicator连线不对的那个bug
                var (tox, toz) = stage.StageManager.ConvertCoordToWorld(NotePrototype.NextLink.PositionCoord - NotePrototype.PositionCoord);
                _linkLineEndOffset = new Vector2(tox, toz);
            }
            else {
                _linkLineEndOffset = null;
            }

            if (NotePrototype.IsHold) {
                _holdBodySpriteRender.gameObject.SetActive(true);
                _holdBodyTransform.WithLocalScaleXY(
                    NotePrototype.Size, 
                    App.GameStageManager.ConvertCoordTimeToWorldScaleY(NotePrototype.Duration, NotePrototype.Speed, true)
                );
            }
            else {
                _holdBodySpriteRender.gameObject.SetActive(false);
            }
        }
    }
}