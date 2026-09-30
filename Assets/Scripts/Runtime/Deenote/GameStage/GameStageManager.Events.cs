using Deenote.Core.GameStage;
using Deenote.GamePlay.UI;

namespace Deenote.GameStage
{
    partial class GameStageManager
    {
        public readonly record struct StageLoadedEventArgs(
            GameStageController Stage
        );
    }
}
