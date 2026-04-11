#nullable enable

using Deenote.Audio;
using Deenote.Contexts;
using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Core.GamePlay.Audio;
using Deenote.Core.Project;
using Deenote.GamePlay;
using Deenote.GameStage;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using Deenote.Library.Components;
using Deenote.ProjectManagement;
using Deenote.Systems;
using NaughtyAttributes;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote
{
    public sealed partial class MainSystem : SingletonBehaviour<MainSystem>
    {
        [Required][SerializeField] GameMusicPlayer _gameMusicPlayer;
        [Required][SerializeField] HitSoundPlayer _hitSoundPlayer;
        [Required][SerializeField] AutoSaveTrigger _autoSaveTrigger = default!;
        [Header("System")]
        [SerializeField] PianoSoundSource _pianoSoundSource = default!;
        [SerializeField] MonoBehaviourHooks _hooks = default!;
        [Header("Manager")]
        [SerializeField] ProjectManager _projectManager = default!;
        [SerializeField] GamePlayManager _gamePlayManager = default!;
        [SerializeField] StageChartEditor _stageChartEditor = default!;

        private UnhandledExceptionHandler _unhandledExceptionHandler = default!;

        public static AutoSaveTrigger AutoSaveTrigger => Instance._autoSaveTrigger;

        public static SaveSystem SaveSystem { get; private set; }
        public static GlobalSettings GlobalSettings { get; private set; }

        public static PianoSoundSource PianoSoundSource => Instance._pianoSoundSource;
        internal static MonoBehaviourHooks GlobalHooks => Instance._hooks;

        public static ProjectManager ProjectManager { get; private set; }
        public static GamePlayManager GamePlayManager => Instance._gamePlayManager;
        public static StageChartEditor StageChartEditor => Instance._stageChartEditor;

        public static RootContext Contexts { get; private set; }
        public static ProjectManagerB ProjectManagerB { get; private set; }
        public static GamePlayManagerB GamePlayManagerB { get; private set; }
        internal static GameStageManager GameStageManager { get; private set; }
        public static GameStageThemeManager GameStageThemeManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _unhandledExceptionHandler = new();

            var stagePianoSoundPlayer = new StagePianoSoundPlayer(PianoSoundSource);

            SaveSystem = new();
            Contexts = new(SaveSystem);
            ProjectManagerB = new(Contexts.Project, Contexts.Environment);
            ProjectManager = new(Contexts.Project, Contexts.Environment);
            GamePlayManagerB = new(Contexts.GamePlay, Contexts.Project, _gameMusicPlayer, stagePianoSoundPlayer, _hitSoundPlayer);
            GameStageThemeManager = new(Contexts.GameStage.ThemeContext);
            GameStageManager = new(Contexts.GameStage, Contexts.Editor, Contexts.GamePlay, GameStageThemeManager);

            GlobalSettings = new();

            GamePlayManager._context = Contexts.GamePlay;
            GamePlayManager._stageContext = Contexts.GameStage;
            GamePlayManager._projectContext = Contexts.Project;

            StageChartEditor.OnInstantiate(ProjectManager, GamePlayManager, Contexts.GameStage);
        }

        private void Start()
        {
            SaveSystem.LoadConfigurations();
            //_ = GameStageSceneLoader.LoadAsync("DeemoStage");
        }

        public static partial class Args
        {
            public const string DeenotePreferFileExtension = ".dnt";
            public const string DeenotePreferChartExtension = ".json";

            public static readonly ImmutableArray<string> SupportLoadProjectFileExtensions
                = ImmutableArray.Create(DeenotePreferFileExtension, ".dsproj");
            public static readonly ImmutableArray<string> SupportLoadAudioFileExtensions
                = ImmutableArray.Create(".mp3", ".wav");
            public static readonly ImmutableArray<string> SupportLoadChartFileExtensions
                = ImmutableArray.Create(".json", ".txt");
        }
    }
}