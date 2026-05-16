#nullable enable

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Deenote.GameStage.UI.Deemo
{
    [MovedFrom("Deenote.Core.GameStage.Foreground")]
    [CreateAssetMenu(
        fileName = nameof(DeemoForegroundUIConfig),
        menuName = $"Deenote/GamePlay/{nameof(DeemoForegroundUIConfig)}")]
    public sealed class DeemoForegroundUIConfig : ScriptableObject
    {
        [Header("Combo")]
        public int MinDisplayCombo;
        [Header("Combo Number")]
        public float ComboNumberGreyIncTime;
        public float ComboNumberGreyDecTime;
        [Header("Combo Number Shadow")]
        public float ComboShadowDuration;
        public float ComboShadowMinAlpha;
        public float ComboShadowMaxScale;
        [Header("Combo Wave")]
        public float ComboCircleScaleStartTime;
        public float ComboCircleScaleEndTime;
        public float ComboCircleMaxScale;
        public float ComboCircleFadeInStartTime;
        public float ComboCircleFadeInEndTime;
        public float ComboCircleFadeOutStartTime;
        [Header("Combo ShockWave")]
        public float ComboShockWaveAlphaIncTime;
        public float ComboShockWaveMoveTime;
        public float ComboShockWaveAlphaDecTime;
        [Obsolete]
        public float ComboShockWaveStartX;
        [Obsolete]
        public float ComboShockWaveEndX;
        [Header("Combo Charming")]
        public float ComboCharmingGrowTime;
        public float ComboCharmingFadeTime;
        public float ComboCharmingAlphaIncTime;
        public float ComboCharmingAlphaDecStartTime;
        public float ComboCharmingAlphaDecTime;
        public float ComboCharmingMaxScaleY;
    }
}