#nullable enable

using Deenote.Core;
using Deenote.GamePlay;
using Deenote.GameStage;

namespace Deenote.Contexts
{
    public sealed class RootContext
    {
        public ProjectContext Project { get; }
        internal EditorContext Editor { get; }
        public EnvironmentContext Environment { get; }
        public GamePlayContext GamePlay { get; }
        public GameStageContext GameStage { get; }

        internal RootContext(SaveSystem storage)
        {
            Environment = new EnvironmentContext(storage);
            Project = new ProjectContext();
            Editor = new EditorContext(Project);
            GamePlay = new GamePlayContext(Project, storage);
            GameStage = new GameStageContext(Project, GamePlay, storage);
        }
    }
}
