using Deenote.Audio;
using Deenote.Core.GamePlay.Audio;
using Deenote.Core.Logging;
using Deenote.CoreB;
using Deenote.GamePlay;
using Deenote.GamePlay.Audio;
using Deenote.GameStage;
using Deenote.GameStage.UI;
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

        private EnvironmentContext _environment;
        public static EnvironmentContext Environment => Current._environment;

        // Only the above services are defined as static

        public ProjectAutoSaveTrigger ProjectAutoSaveTrigger { get; private set; }

        public StagePianoSoundPlayer PianoSoundPlayer;
        public HitSoundPlayer HitSoundPlayer;

        private App()
        {
        }

        public static App Create(
            IGameStagePerspectiveViewPanel foreground,
            GameMusicPlayer gameMusicPlayer,
            PianoSoundSource pianoSoundSource,
            HitSoundSource hitSoundSource,
            HitSoundPlayer hitSoundPlayer)
        {
            Current = new App();
            Application.Current = Current;
            Current._gamePlayerManager = new GamePlayManager2(gameMusicPlayer, pianoSoundSource, hitSoundSource);
            Current._projectManager = new ProjectManager2();
            Current._gameStageManager = new GameStageManager(foreground);
            Current._environment = new EnvironmentContext();
            Current.ProjectAutoSaveTrigger = new ProjectAutoSaveTrigger();

            var stagePianoSoundPlayer = new StagePianoSoundPlayer(pianoSoundSource);
            Current.PianoSoundPlayer = stagePianoSoundPlayer;
            Current.HitSoundPlayer = hitSoundPlayer;

            return Current;
        }
    }
}