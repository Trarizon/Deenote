#nullable enable

using Deenote.CoreB.Unity;
using Deenote.GameStage.World.Deemo.Notes;
using Deenote.Library;
using TriInspector;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class DeemoGameStageSpeedChangeWarningNoteController : GameStageSpeedChangeWarningNoteController
    {
        [SerializeField] private SpriteRenderer _noteSpriteRenderer = default!;

        [Title("Config")]
        [SerializeField] internal DeemoStageNoteConfig _config;

        protected override void SetNoteSprite()
        {
            // _noteSpriteRenderer.sprite = _game.Stage!.Args.SpeedChangeWarningSpritePrefab.Sprite; 
        }

        protected override void SetNoteSize()
        {
            // var prefab = _game.Stage!.Args.SpeedChangeWarningSpritePrefab;
            // _noteSpriteRenderer.transform.localScale = Vector3.one * prefab.Scale;
        }

        protected override void SetNoteSpriteRendererAlpha(float alpha)
        {
            _noteSpriteRenderer.WithColorAlpha(alpha);
        }

        protected override void SetNoteSpriteEditorStatus(bool selected)
        {
            _noteSpriteRenderer.WithColorRGB(selected ? _config.SelectedHighlightColor : Color.white);
        }

        protected override void SetBodyActive(bool active)
        {
            _noteSpriteRenderer.gameObject.SetActive(active);
        }
    }
}
