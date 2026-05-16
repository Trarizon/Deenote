#nullable enable

using Deenote.CoreB.Models;
using System;
using UnityEngine;

namespace Deenote.GameStage.UI
{
    [CreateAssetMenu(
        fileName = "GameStageUIConfig",
        menuName = $"Deenote/GameStage/UIConfig")]
    public sealed class GameStageUIConfig : ScriptableObject
    {
        public DifficultyData EasyData;
        public DifficultyData NormalData;
        public DifficultyData HardData;
        public DifficultyData ExtraData;
        public DifficultyData SpecialData;

        public DifficultyData Get(Difficulty difficulty) => difficulty switch {
            Difficulty.Easy => EasyData,
            Difficulty.Normal => NormalData,
            Difficulty.Hard => HardData,
            Difficulty.Extra => ExtraData,
            Difficulty.Special => SpecialData,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };

        [Serializable]
        public struct DifficultyData
        {
            public Sprite IconSprite;
            public Color TextColor;
        }
    }
}
