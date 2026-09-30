using Deenote.Core.Logging;
using Deenote.CoreB;
using Deenote.GameStage;
using Deenote.GameStage.UI;

namespace Deenote
{
    public class App : Application
    {
        public static new App Current { get; private set; } = default!;
        public static new Logger Logger => ((Application)Current).Logger;

        private GameStageManager _gameStageManager;
        public static GameStageManager GameStageManager => Current._gameStageManager;

        private App() { }

        public static App Create(IGameStagePerspectiveViewPanel foreground)
        {
            var app = new App {
                _gameStageManager = new GameStageManager(foreground)
            };
            return Current = app;
        }
    }
}