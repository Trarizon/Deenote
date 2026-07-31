using System;
using TriInspector;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.World.Deemo.Notes
{
    [MovedFrom(true, sourceClassName: "DeemoGameStageNoteConfig")]
    [CreateAssetMenu(
        fileName = "DeemoGameStageNoteConfig",
        menuName = "Deenote/GameStage/Deemo/NoteConfig"
    )]
    internal class DeemoStageNoteConfig : ScriptableObject
    {
        [Title("Note Sprites")]
        public NoteSpriteData ClickNoteSpriteData;
        public NoteSpriteData NoSoundNoteSpriteData;
        public NoteSpriteData SlideNoteSpriteData;
        public NoteSpriteData SwipeNoteSpriteData;

        [Title("Hold Note")]
        public Sprite HoldSprite;
        public Color HoldingColor;

        [Title("Hit Effect")]
        public Sprite[] ExplosionSprites;
        public float ExplosionAnimationDuration;
        public float ExplosionScale;
        public float ShockwaveScale;
        public float CirclewaveScale;
        public Vector2 GlowScale;

        [Title("Highlight")]
        public Color SelectedHighlightColor;
        public Color CollidedHighlightColor;
        public Color SelectedAndCollidedHighlightColor;
        [Range(0, 1)]
        public float DownplayAlpha;

        [Serializable]
        public struct NoteSpriteData
        {
            public float Scale;
            public Sprite Sprite;
            public Color WaveColor;
        }
    }
}
