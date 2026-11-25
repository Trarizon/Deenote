using Cysharp.Threading.Tasks;
using Deenote.Core.GameStage;
using Deenote.CoreB.Notification;
using Deenote.Editing;
using Deenote.EditorModels;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using Deenote.GameStage.UI;
using Deenote.GameStage.World;
using Deenote.Project;
using System;

namespace Deenote.GameStage
{
    public sealed partial class GameStageManager
    {
        private readonly IGameStagePerspectiveViewPanel _foreground;

        private readonly ProjectManager2 _projectManager;
        private readonly GamePlayManager2 _gamePlayManager;
        private readonly StageNotesPlacer2 _notePlacer;

        public IGameStagePerspectiveViewPanel ForegroundViewPanel => _foreground;
        public GameStageThemeManager ThemeManager { get; }
        public GameStageGridsManager GridsManager { get; }
        public GameStageNotesManager NotesManager { get; private set; }
        public GameStageWarningNotesManager WarningNotesManager { get; private set; } = default!;

        public GameStageConfig Config => GameStage!.Config;
        public IGameStageStrategy Strategy { get; } = new DeemoGameStageStrategy();
        private IGameStageNoteFactory? NoteFactory { get; set; }

        public GameStageController? GameStage => ThemeManager.CurrentStage;

        internal GameStageManager(IGameStagePerspectiveViewPanel foreground,
            ProjectManager2 projectManager, GamePlayManager2 gamePlayManager, StageNotesPlacer2 notePlacer, AppGlobalConfigs appConfig)
        {
            _foreground = foreground;
            _projectManager = projectManager;
            _gamePlayManager = gamePlayManager;
            _notePlacer = notePlacer;

            ThemeManager = new(appConfig.BuiltinStageThemes);
            GridsManager = new();
            NotesManager = new(this, gamePlayManager, projectManager);


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
                    App.Logger.LogTrace($"Stage theme changed, sync foreground view");
                    _foreground.ApplyStageAsync(async parent =>
                    {
                        return await s.LoadCurrentThemeForegroundAsync(parent);
                    });
                }
            });
            _projectManager.RegisterPropertyChanged((s, e) =>
            {
                if (e.Match(nameof(s.CurrentChart))) {
                    if (s.CurrentChart is not null)
                        CheckCollision(s.CurrentChart);
                    NotesManager.RefreshActiveNotes();
                    WarningNotesManager.RefreshActiveNotes();
                }
            });

            App.Current.UnscaledTick += e =>
            {
                // chino added this to fix preview stutter, while this looks weird to call every frame
                foreach (var note in _activeNotes) {
                    note.RefreshStageDeltaTime();
                }
                WarningNotesManager.RefreshActiveNotes();
            };
        }

        public void PostConstructor()
        {
            WarningNotesManager = new(this, _gamePlayManager, _projectManager);

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
            if (NoteFactory is not null) {
                foreach (var note in _activeNotes)
                    NoteFactory.Release(note);
                foreach (var warningNote in _activeWarningNotes)
                    NoteFactory.Release(warningNote);
                foreach (var indicator in _activeIndicators)
                    NoteFactory.Release(indicator);
                _activeNotes.Clear();
                _activeWarningNotes.Clear();
                _activeIndicators.Clear();
                NoteFactory.Dispose();
            }
            NoteFactory = new DefaultGameStageNoteFactory(Config, stage);
            RefreshStageVisibleNotes(NotesManager.ActiveNotes);
            RefreshStageVisibleWarningNotes(WarningNotesManager.ActiveNotes);
        }

        private void RefreshStageVisibleNotes()
        {
            RefreshStageVisibleNotes(NotesManager.ActiveNotes);
            RefreshStageVisibleWarningNotes(WarningNotesManager.ActiveNotes);
        }

        private void CheckCollision(ChartEditorModel chart)
        {
            for (int i = 0; i < chart.NoteNodes.Count; i++) {
                if (chart.NoteNodes[i] is not NoteModel note)
                    continue;

                for (int j = i + 1; j < chart.NoteNodes.Count; j++) {
                    if (chart.NoteNodes[j] is not NoteModel noteCmp)
                        continue;

                    if (!EntityArgs.IsTimeCollided(note, noteCmp))
                        break;
                    if (EntityArgs.IsPositionCollided(note, noteCmp)) {
                        note.CollisionCount++;
                        noteCmp.CollisionCount++;
                    }
                }
            }
        }
    }
}