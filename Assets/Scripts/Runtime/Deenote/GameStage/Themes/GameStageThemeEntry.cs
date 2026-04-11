#nullable enable

using Deenote.Core.GameStage;
using Deenote.Core.GameStage.Foreground;
using Deenote.GameStage.Grids;
using System;
using UnityEngine;

namespace Deenote.GameStage.Themes
{
    public sealed class GameStageThemeEntry
    {
        internal GameStageThemeConfig ThemeConfig { get; private set; }
        public GameStageController Stage { get; private set; }
        public IGameStageNoteCoordStrategy NoteCoordStrategy { get; private set; }
        internal IGameStageNoteFactory NoteFactory { get; private set; }

        internal IGameStageConfig Config { get; }

        internal GridLineConfig GridLineConfig => ThemeConfig.GridLineConfig;

        [Obsolete("Temporary for GamePlayManager")]
        internal ForegroundPerspectiveViewUI PerspectiveViewForeground => ThemeConfig.GameUIPrefab;

        internal GameStageThemeEntry(GameStageThemeConfig themeConfig, GameStageController stage)
        {
            ThemeConfig = themeConfig;
            Stage = stage;
            Config = new DeemoGameStageConfig(stage.Config);
            NoteCoordStrategy = new DefaultGameStageNoteCoordStrategy(stage.Config);
            NoteFactory = new DefaultGameStageNoteFactory(themeConfig.NotePrefab, stage.NotePlane);
        }

        public ForegroundPerspectiveViewUI InstantiateUIAsync(Transform parent)
        {
            return UnityEngine.Object.Instantiate(ThemeConfig.GameUIPrefab, parent);
        }
    }
}