using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.Core.Logging;
using Deenote.CoreB;
using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.GameStage.UI;
using Deenote.Inputting;
using Deenote.Project;
using Deenote.Systems;

namespace Deenote
{
    public partial class App : Application
    {
        public static new App Current { get; private set; } = default!;

        private GamePlayManager2 _gamePlayerManager;
        public static GamePlayManager2 GamePlayManager => Current._gamePlayerManager;

        private GameStageManager _gameStageManager;
        public static GameStageManager GameStageManager => Current._gameStageManager;

        private ProjectManager2 _projectManager;
        public static ProjectManager2 ProjectManager => Current._projectManager;

        private EditingManager _editingManager;
        public static EditingManager EditingManager => Current._editingManager;

        private EnvironmentContext _environment;
        public static EnvironmentContext Environment => Current._environment;

        // Only the above services are defined as static

        public ProjectAutoSaveTrigger ProjectAutoSaveTrigger { get; private set; }

        public StagePianoSoundPlayer PianoSoundPlayer;
        public HitSoundSource HitSoundPlayer;

        private InputInterpreter _inputInterpreter;

        private App()
        {
        }

        public static App Create(
            AppGlobalConfigs appGlobalConfigs,
            IGameStagePerspectiveViewPanel foreground,
            IMouseInputHandler mouseInputHandler,
            GameMusicPlayer gameMusicPlayer,
            PianoSoundSource pianoSoundSource,
            HitSoundSource hitSoundSource)
        {
            Current = new App();
            Application.Current = Current;
            Current.Configs = appGlobalConfigs;
            
            
            Current._environment = new EnvironmentContext();

            Current._projectManager = new ProjectManager2();
            Current._gamePlayerManager = new GamePlayManager2(
                gameMusicPlayer, pianoSoundSource, hitSoundSource,
                Current._projectManager);
            Current._gameStageManager = new GameStageManager(
                foreground,
                Current._projectManager, Current._gamePlayerManager, appGlobalConfigs);
            Current._editingManager = new EditingManager(
                Current._projectManager, Current._gameStageManager, Current._gamePlayerManager);

            Current.ProjectAutoSaveTrigger = new ProjectAutoSaveTrigger();

            var stagePianoSoundPlayer = new StagePianoSoundPlayer(pianoSoundSource);
            Current.PianoSoundPlayer = stagePianoSoundPlayer;
            // Current.HitSoundPlayer = hitSoundPlayer;

            Current._inputInterpreter = new InputInterpreter(
                Current._gamePlayerManager,
                Current._gameStageManager,
                Current._projectManager,
                Current._editingManager,
                Current._environment,
                mouseInputHandler,
                foreground
            );

            return Current;
        }
    }
}