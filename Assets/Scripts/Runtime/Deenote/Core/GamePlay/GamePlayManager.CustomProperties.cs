#nullable enable

using Deenote.Library;
using Deenote.Library.Mathematics;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        private void RegisterCustomPropertiesConfigurations()
        {
            //MainSystem.SaveSystem.SavingConfigurations += configs =>
            //{
            //    configs.Set("stage/line-color-subbeat", CustomSubBeatLineColor?.ToRgbaString());
            //    configs.Set("stage/line-color-beat", CustomBeatLineColor?.ToRgbaString());
            //    configs.Set("stage/line-color-tempo", CustomTempoLineColor?.ToRgbaString());
            //};
            //MainSystem.SaveSystem.LoadedConfigurations += configs =>
            //{
            //    if (ColorUtils.TryParse(configs.GetString("stage/line-color-subbeat"), out var sbc)) {
            //        CustomSubBeatLineColor = sbc;
            //    }
            //    if (ColorUtils.TryParse(configs.GetString("stage/line-color-beat"), out var bc)) {
            //        CustomBeatLineColor = bc;
            //    }
            //    if (ColorUtils.TryParse(configs.GetString("stage/line-color-tempo"), out var tc)) {
            //        CustomTempoLineColor = tc;
            //    }
            //};
        }

        public Color? CustomSubBeatLineColor
        {
            get => _stageContext.CustomSubBeatLineColor;
            set {
                _stageContext.CustomSubBeatLineColor = value;
                NotifyFlag(NotificationFlag.CustomSubBeatLineColor);
            }
        }
        public Color? CustomBeatLineColor
        {
            get => _stageContext.CustomBeatLineColor;
            set {
                _stageContext.CustomBeatLineColor = value;
                NotifyFlag(NotificationFlag.CustomBeatLineColor);
            }
        }
        public Color? CustomTempoLineColor
        {
            get => _stageContext.CustomTempoLineColor;
            set {
                _stageContext.CustomTempoLineColor = value;
                NotifyFlag(NotificationFlag.CustomTempoLineColor);
            }
        }
    }
}