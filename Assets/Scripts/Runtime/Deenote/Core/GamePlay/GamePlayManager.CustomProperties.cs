#nullable enable

using Deenote.Library.Mathematics;
using System;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        private void RegisterCustomPropertiesConfigurations()
        {
            // MainSystem.SaveSystem.SavingConfigurations += configs =>
            // {
            //     configs.Set("stage/line-color-subbeat", CustomSubBeatLineColor?.ToRGBAString());
            //     configs.Set("stage/line-color-beat", CustomBeatLineColor?.ToRGBAString());
            //     configs.Set("stage/line-color-tempo", CustomTempoLineColor?.ToRGBAString());
            // };
            // MainSystem.SaveSystem.LoadedConfigurations += configs =>
            // {
            //     if (ColorUtils.TryParse(configs.GetString("stage/line-color-subbeat"), out var sbc)) {
            //         CustomSubBeatLineColor = sbc;
            //     }
            //     if (ColorUtils.TryParse(configs.GetString("stage/line-color-beat"), out var bc)) {
            //         CustomBeatLineColor = bc;
            //     }
            //     if (ColorUtils.TryParse(configs.GetString("stage/line-color-tempo"), out var tc)) {
            //         CustomTempoLineColor = tc;
            //     }
            // };
        }

        [Obsolete("Moved to GameStageManager")]
        public Color? CustomSubBeatLineColor
        {
            get => App.GameStageManager.CustomSubdivisionLineColor;
            set {
                App.GameStageManager.CustomSubdivisionLineColor = value;
                NotifyFlag(NotificationFlag.CustomSubBeatLineColor);
            }
        }

        [Obsolete("Moved to GameStageManager")]
        public Color? CustomBeatLineColor
        {
            get => App.GameStageManager.CustomBeatLineColor;
            set {
                App.GameStageManager.CustomBeatLineColor = value;
                NotifyFlag(NotificationFlag.CustomBeatLineColor);
            }
        }

        [Obsolete("Moved to GameStageManager")]
        public Color? CustomTempoLineColor
        {
            get => App.GameStageManager.CustomTempoLineColor;
            set {
                App.GameStageManager.CustomTempoLineColor = value;
                NotifyFlag(NotificationFlag.CustomTempoLineColor);
            }
        }
    }
}