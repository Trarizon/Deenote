#nullable enable

using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.GameStage;

namespace Deenote.Contexts
{
    public sealed class RootContext
    {
        public ProjectContext Project { get; internal init; }
        public EditorContext Editor { get; internal init; }
        public EnvironmentContext Environment { get; internal init; }
        public GamePlayContext GamePlay { get; internal init; }
        public GameStageContext GameStage { get; internal init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal RootContext() { }
#pragma warning restore CS8618
    }
}
