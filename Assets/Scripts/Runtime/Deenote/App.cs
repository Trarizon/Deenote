using Deenote.Core.Logging;
using Deenote.CoreB;
using Deenote.GameStage;
using Deenote.GameStage.UI;
using Deenote.Project;
using Deenote.Systems;

namespace Deenote
{
    public class App : Application
    {
        public static new App Current { get; private set; } = default!;
        public static new Logger Logger => ((Application)Current).Logger;

        private GameStageManager _gameStageManager;
        public static GameStageManager GameStageManager => Current._gameStageManager;

        private ProjectManager2 _projectManager;
        public static ProjectManager2 ProjectManager => Current._projectManager;

        private EnvironmentContext _environment;
        public static EnvironmentContext Environment => Current._environment;

        // Only the above services are defined as static

        public ProjectAutoSaveTrigger ProjectAutoSaveTrigger { get; private set; }

        private App() { }

        public static App Create(IGameStagePerspectiveViewPanel foreground)
        {
            Current = new App();
            Application.Apply(Current);
            Current._projectManager = new ProjectManager2();
            Current._gameStageManager = new GameStageManager(foreground);
            Current._environment = new EnvironmentContext();
            Current.ProjectAutoSaveTrigger = new ProjectAutoSaveTrigger();
            return Current;
        }
    }
}