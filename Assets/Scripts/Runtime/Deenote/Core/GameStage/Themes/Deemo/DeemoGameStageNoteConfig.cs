#nullable enable

using System;
using UnityEngine;

namespace Deenote.Core.GameStage.Themes.Deemo
{
    [CreateAssetMenu(
        fileName = nameof(DeemoGameStageNoteConfig),
        menuName = "Deenote/GameStage/Deemo/NoteConfig")]
    internal sealed class DeemoGameStageNoteConfig : ScriptableObject
    {
        public NoteSpriteData ClickNoteSpriteData;
        public NoteSpriteData NoSoundNoteSpriteData;
        public NoteSpriteData SlideNoteSpriteData;
        public NoteSpriteData SwipeNoteSpriteData;

        [Header("Hold Note Body")]
        public float HoldScaleX; // Not used, I directly set it in inspector
        public Sprite HoldSprite;
        public Color HoldingBodyColor;

        [Header("Hit Effect")]
        public Sprite[] ExplosionSprites;
        public float ExplosionAnimationDuration;
        public float ExplosionScale;
        public float ShockwaveScale;
        public float CirclewaveScale;
        public Vector2 GlowScale;

        [Header("Highlight")]
        public Color SelectedHighlightColor;
        public Color CollidedHighlightColor;
        public Color SelectedAndCollidedHighlightColor;
        [Range(0f, 1f)]
        public float DownplayAlpha = 0.25f;

        [Serializable]
        public struct NoteSpriteData
        {
            public float Scale;
            public Sprite Sprite;
            public Color WaveColor;
        }
    }
}
