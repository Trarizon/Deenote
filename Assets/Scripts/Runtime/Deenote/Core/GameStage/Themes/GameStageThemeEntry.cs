#nullable enable

using Deenote;
using Deenote.Core.GameStage.Foreground;
using UnityEngine;

namespace Deenote.Core.GameStage.Themes
{
    public sealed class GameStageThemeEntry
    {
        internal GameStageThemeConfig ThemeConfig { get; private set; }
        public GameStageController Stage { get; private set; }
        public IGameStageNoteCoordStrategy NoteCoordStrategy { get; private set; }

        internal GameStageThemeEntry(GameStageThemeConfig config,GameStageController stage)
        {
            ThemeConfig = config;
            Stage = stage;
            NoteCoordStrategy = new DefaultGameStageNoteCoordStrategy(stage.Config);
        }

        public ForegroundPerspectiveViewUI InstantiateUIAsync(Transform parent)
        {
            return Object.Instantiate(ThemeConfig.GameUIPrefab, parent);
        }
    }
}