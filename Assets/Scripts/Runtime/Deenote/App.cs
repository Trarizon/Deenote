using Deenote.GameStage;

namespace Deenote
{
    public class App
    {
        public static App Current { get; private set; } = default!;

        internal App()
        {
            Current = this;
            
            _gameStageManager = new GameStageManager();
        }

        private GameStageManager _gameStageManager;
        public static GameStageManager GameStageManager => Current._gameStageManager;

    }
}