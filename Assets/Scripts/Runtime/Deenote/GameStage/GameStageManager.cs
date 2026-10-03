using Cysharp.Threading.Tasks;
using Deenote.Core.GameStage;
using Deenote.GameStage.Grids;
using Deenote.GameStage.Themes;
using Deenote.GameStage.UI;
using Deenote.GameStage.World;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.GameStage
{
    public sealed partial class GameStageManager
    {
        private readonly IGameStagePerspectiveViewPanel _foreground;

        public GameStageGridsManager GridsManager { get; }
        public GameStageNotesManager NotesManager { get; private set; } = default;

        public GameStageConfig Config => GameStage!.Config;
        public IGameStageStrategy Strategy { get; } = new DeemoGameStageStrategy();
        private IGameStageNoteFactory? NoteFactory { get; set; }

        public GameStageController? GameStage { get; private set; }


        public event Action<StageLoadedEventArgs>? StageLoaded;

        internal GameStageManager(IGameStagePerspectiveViewPanel foreground)
        {
            _foreground = foreground;
            GridsManager = new();


            GameStageSceneLoader.StageLoaded += (loader) =>
            {
                Core(loader).Forget();
                async UniTaskVoid Core(GameStageSceneLoader loader)
                {
                    GameStage = loader.StageController;

                    MainSystem.GamePlayManager.OnStageLoaded(loader);
                    OnGameStageLoaded(loader.StageController);

                    GameStage.PerspectiveCamera.ApplyToRenderTexture(_foreground.ViewRendererTexture);

                    await _foreground.ApplyStageAsync(parent =>
                    {
                        return UniTask.FromResult<GameStageForegroundView>(UnityEngine.Object.Instantiate(loader.PerspectiveViewForeground, parent));
                    });

                    StageLoaded?.Invoke(new StageLoadedEventArgs(GameStage));
                }
            };
        }

        public void PostConstructor()
        {
            NotesManager = new(this, MainSystem.GamePlayManager, App.ProjectManager);

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