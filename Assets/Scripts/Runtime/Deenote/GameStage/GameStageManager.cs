using Deenote.Core.GameStage;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using Deenote.GameStage.World;

namespace Deenote.GameStage
{
    public sealed partial class GameStageManager
    {
        public GameStageGridsManager GridsManager { get; }
        public GameStageNotesManager NotesManager { get; }

        public GameStageConfig Config => MainSystem.GamePlayManager.Stage!.Config;
        public IGameStageStrategy Strategy { get; } = new DeemoGameStageStrategy();
        private IGameStageNoteFactory? NoteFactory { get; set; }

        public GameStageController? GameStage { get; private set; }

        internal GameStageManager()
        {
            GridsManager = new();
            NotesManager = new(this, MainSystem.ProjectManager);

            Ctor_Notes();

            GameStageSceneLoader.StageLoaded += loader =>
            {
                // GameStage = loader.StageController;
                OnGameStageLoaded(loader.StageController);
            };
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

        private void OnGameStageLoaded(GameStageController stage)
        {
            NoteFactory = new DefaultGameStageNoteFactory(
                Config.NotePrefab,
                stage.NotePlane
            );
            RefreshStageVisibleNotes(NotesManager.ActiveNotes);
        }
    }
}