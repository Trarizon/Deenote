#nullable enable

using Deenote.Audio;
using Deenote.Contexts;
using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.Core.Project;
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
        [Required][SerializeField] AutoSaveTrigger _autoSaveTrigger = default!;
        [Header("System")]
        [SerializeField] PianoSoundSource _pianoSoundSource = default!;
        [Header("Manager")]
        [SerializeField] ProjectManager _projectManager = default!;
        [SerializeField] GamePlayManager _gamePlayManager = default!;
        [SerializeField] StageChartEditor _stageChartEditor = default!;

        private UnhandledExceptionHandler _unhandledExceptionHandler = default!;

        public static AutoSaveTrigger AutoSaveTrigger => Instance._autoSaveTrigger;

        public static SaveSystem SaveSystem { get; private set; } 
        public static GlobalSettings GlobalSettings { get; private set; }

        public static PianoSoundSource PianoSoundSource => Instance._pianoSoundSource;

        public static ProjectManager ProjectManager { get; private set; }
        public static GamePlayManager GamePlayManager => Instance._gamePlayManager;
        public static StageChartEditor StageChartEditor => Instance._stageChartEditor;

        public static RootContext Contexts { get; private set; }
        public static ProjectManagerB ProjectManagerB { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            SaveSystem = new();
            Contexts = new(SaveSystem);
            ProjectManagerB = new(Contexts.Project, Contexts.Environment);
            ProjectManager = new(Contexts.Project, Contexts.Environment);

            _unhandledExceptionHandler = new();


            GlobalSettings = new();

            StageChartEditor.OnInstantiate(ProjectManager, GamePlayManager);
        }

        private void Start()
        {
            SaveSystem.LoadConfigurations();
            _ = GameStageSceneLoader.LoadAsync("DeemoStage");
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