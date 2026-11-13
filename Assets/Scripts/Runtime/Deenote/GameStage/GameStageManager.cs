using Cysharp.Threading.Tasks;
using Deenote.Core.GameStage;
using Deenote.CoreB.Notification;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using Deenote.GameStage.UI;
using Deenote.GameStage.World;
using System;

namespace Deenote.GameStage
{
    public sealed partial class GameStageManager
    {
        private readonly IGameStagePerspectiveViewPanel _foreground;

        public IGameStagePerspectiveViewPanel ForegroundViewPanel => _foreground;
        public GameStageThemeManager ThemeManager { get; }
        public GameStageGridsManager GridsManager { get; }
        public GameStageNotesManager NotesManager { get; private set; }
        public GameStageWarningNotesManager WarningNotesManager { get; private set; } = default!;

        public GameStageConfig Config => GameStage!.Config;
        public IGameStageStrategy Strategy { get; } = new DeemoGameStageStrategy();
        private IGameStageNoteFactory? NoteFactory { get; set; }

        public GameStageController? GameStage => ThemeManager.CurrentStage;

        internal GameStageManager(IGameStagePerspectiveViewPanel foreground, AppGlobalConfigs appConfig)
        {
            _foreground = foreground;
            ThemeManager = new(appConfig.BuiltinStageThemes);
            GridsManager = new();


            ThemeManager.RegisterPropertyChanged((s, e) =>
            {
                if (e.Match(nameof(s.CurrentStage))) {
                    PropertyChanged?.Invoke(this, nameof(GameStage));

                    if (s.CurrentStage is { } stage) {
                        MainSystem.GamePlayManager.OnStageLoaded(stage);
                        OnGameStageLoaded(stage);

                        stage.OnInstantiate(this);
                    }
                }
                if (e.Match(nameof(s.CurrentTheme))) {
                    _foreground.ApplyStageAsync(async parent =>
                    {
                        return await s.LoadCurrentThemeForegroundAsync(parent);
                    });
                }
            });
        }

        public void PostConstructor()
        {
            NotesManager = new(this, App.GamePlayManager, App.ProjectManager);
            WarningNotesManager = new(this, App.GamePlayManager, App.ProjectManager);

            RegisterConfigurations();
            RegisterCustomPropertiesConfigurations();

            Ctor_Notes();
        }

        /// <summary>
        /// For a note that speed = 1, the duration from the note activated to the note reach the judge line
        /// </summary>
        public float StandardNoteActiveAheadTime
        {
            get {
                var fSpeed = Strategy.GetNoteFallSpeedInternal(this.ActualNoteFallSpeed);
                return Config.NoteAppearAheadTimeFactor / fSpeed;
            }
        }

        public float GetNoteActiveAheadTime(float noteSpeed) => StandardNoteActiveAheadTime / noteSpeed;

        private void OnGameStageLoaded(GameStageController stage)
        {
            NoteFactory = new DefaultGameStageNoteFactory(
                Config,
                stage.NotePlane
            );
            RefreshStageVisibleNotes(NotesManager.ActiveNotes);
            RefreshStageVisibleWarningNotes(WarningNotesManager.ActiveNotes);
        }
    }
}